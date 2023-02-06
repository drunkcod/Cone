using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CheckThat.Internals;

namespace CheckThat.Expressions
{
	class ExpressionEvaluatorContext 
	{
		readonly Expression context;
		readonly ExpressionEvaluatorParameters parameters;

		readonly Func<Expression, ExpressionEvaluatorParameters,EvaluationResult> Unsupported;
		public Func<Expression, Expression, EvaluationResult> NullSubexpression;

		public ExpressionEvaluatorContext(Expression context, ExpressionEvaluatorParameters parameters, Func<Expression, ExpressionEvaluatorParameters, EvaluationResult> onUnsupported) {
			this.context = context;
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters), new StackTrace().ToString());
			this.Unsupported = onUnsupported;
		}

		public EvaluationResult Evaluate(Expression body) => body.NodeType switch {
			ExpressionType.Lambda => Lambda((LambdaExpression)body),
			ExpressionType.ArrayIndex => ArrayIndex(body),
			ExpressionType.ArrayLength => ArrayLength(body),
			ExpressionType.Call => Call((MethodCallExpression)body),
			ExpressionType.Constant => Success(body.Type, ((ConstantExpression)body).Value),
			ExpressionType.Convert => Convert((UnaryExpression)body),
			ExpressionType.Equal => Binary((BinaryExpression)body),
			ExpressionType.NotEqual => Binary((BinaryExpression)body),
			ExpressionType.MemberAccess => MemberAccess((MemberExpression)body),
			ExpressionType.New => New((NewExpression)body),
			ExpressionType.NewArrayInit => NewArrayInit((NewArrayExpression)body),
			ExpressionType.Quote => Quote((UnaryExpression)body),
			ExpressionType.Invoke => Invoke((InvocationExpression)body),
			ExpressionType.AndAlso => AndAlso((BinaryExpression)body),
			ExpressionType.Parameter => Parameter((ParameterExpression)body),
			_ => Unsupported(body, parameters),
		};

		public EvaluationResult EvaluateAsTarget(Expression expression) {
			if(expression == null)
				return Success(null, null);
			return Evaluate(expression)
				.Then(result => result.IsNull ? NullSubexpression(expression, context) : result);
		}

		public EvaluationResult EvaluateAll(ICollection<Expression> expressions) {
			var r = new object[expressions.Count];
			var i = 0;
			foreach(var item in expressions) {
				var e = Evaluate(item);
				if(e.IsError)
					return e;
				r[i++] = e.Result;
			}
			return Success(typeof(object[]), r);
		}

		EvaluationResult Lambda(LambdaExpression expression) {
			if(expression == context && expression.Parameters.Count == 0)
				return Evaluate(expression.Body);
			return Success(expression.Type, expression.Compile());
		}

		EvaluationResult ArrayIndex(Expression expression) {
			var rank1 = expression as BinaryExpression;
			if(rank1 != null)
				return EvaluateArrayIndex1(rank1);
			return Unsupported(expression, parameters);
		}

		EvaluationResult EvaluateArrayIndex1(BinaryExpression rank1) {
			var array = (Array)Evaluate(rank1.Left).Result;
			var index = (int)Evaluate(rank1.Right).Result;
			if(index >= 0 && index <= array.Length)
				return Success(rank1.Type, array.GetValue(index));
			return Failure(rank1, new IndexOutOfRangeException($"Tried to access element {index} target has only {array.Length} elements."));
		}

		EvaluationResult ArrayLength(Expression expression) {
			var target = Evaluate(((UnaryExpression)expression).Operand);
			if(target.IsNull)
				return Failure(expression, 
					((UnaryExpression)expression).Operand is MemberExpression x 
					? new NullSubexpressionException(x.Member.Name, expression, context)
					: new NullSubexpressionException(expression, context));
			var array = (Array)target.Result;
			return Success(expression.Type, array.Length);
		}

		EvaluationResult Binary(BinaryExpression binary)  => 
			Evaluate(binary.Left)
				.Then<object>(left => Evaluate(binary.Right)
				.Then<object>(right => Binary(binary, left, right)));

		EvaluationResult Binary(BinaryExpression binary, object left, object right) {
			var op = binary.Method;
			if(op != null)
				return Success(op.ReturnType, op.Invoke(null, new []{ left, right }));
			switch(binary.NodeType) {
				case ExpressionType.Equal: return Success(typeof(bool), object.Equals(left, right));
				case ExpressionType.NotEqual: return Success(typeof(bool), !object.Equals(left, right));
				default: return Unsupported(binary, parameters);
			}
		}

		EvaluationResult Call(MethodCallExpression expression) =>
			EvaluateAsTarget(expression.Object).Then<object>(target => {
				var method = expression.Method;
				var ps = method.GetParameters();

				if (HasByRefLikeParameter(ps))
					return InvokeWithByRefArgs(target, expression);

				return EvaluateAll(expression.Arguments).Then<object[]>(input => {
					return GuardedInvocation(expression,
						() => Success(method.ReturnType, method.Invoke(target, input)),
						() => AssignOutParameters(expression.Arguments, input, ps));
				});
			});

		static readonly Func<ParameterInfo[], bool> HasByRefLikeParameter = Lambdas.TryGetProperty<Type, bool>("IsByRefLike", out var isByRefLike) 
			? xs => xs.Any(x => isByRefLike(x.ParameterType))
			: _ => false;

		EvaluationResult InvokeWithByRefArgs(object target, MethodCallExpression expression) {
			var args = new object[parameters.Count + 1];
			var ps = new List<ParameterExpression>(parameters.Count + 1) { 
				Expression.Parameter(expression.Method.DeclaringType) 
			};

			args[0] = target;
			foreach(var item in parameters) {
				args[ps.Count] = item.Value;
				ps.Add(item.Key);
			}

			var call =
				Expression.Lambda(
					Expression.Call(expression.Method.IsStatic ? null : ps[0], expression.Method, expression.Arguments), ps)
				.Compile();
			return GuardedInvocation(expression,
				() => Success(expression.Method.ReturnType, call.DynamicInvoke(args)),
				() => { });

		}

		void AssignOutParameters(IReadOnlyList<Expression> arguments, object[] results, ParameterInfo[] parameters) {
			if(results.Length == 0)
				return;
			for(int i = 0; i != parameters.Length; ++i)
				if(parameters[i].IsOut) {
					var member = arguments[i] as MemberExpression;
					var field = member.Member as FieldInfo;
					field.SetValue(EvaluateMember(member), results[i]);
				}
		}

		object EvaluateMember(MemberExpression member) {
			var e = member.Expression;
			if(e == null)
				return null;
			return Rebind(e).Evaluate(e).Result;
		}

		EvaluationResult Convert(UnaryExpression expression) {
			return Evaluate(expression.Operand).Then<object>(value => {
				var convertMethod = expression.Method;
				if(convertMethod != null && convertMethod.IsStatic) {
					try {
						return Success(convertMethod.ReturnType, convertMethod.Invoke(null, new[] { value }));
					} catch (TargetInvocationException e) {
						return Failure(expression, e.InnerException);
					}
				}
				return Success(expression.Type, ChangeType(value, expression.Type));
			});
		}

		EvaluationResult MemberAccess(MemberExpression expression) => GuardedInvocation(expression, MemberAccessCore);
		EvaluationResult MemberAccessCore(MemberExpression expression) =>
			EvaluateAsTarget(expression.Expression)
			.Then<object>(x => Success(expression.Type, expression.Member.GetValue(x)));

		EvaluationResult New(NewExpression expression) => GuardedInvocation(expression, NewCore);
		EvaluationResult NewCore(NewExpression expression) {
			var args = EvaluateAll(expression.Arguments).Result as object[];
			if (expression.Constructor != null)
				return Success(expression.Type, expression.Constructor.Invoke(args));
			return Success(expression.Type, Activator.CreateInstance(expression.Type, args));
		}

		EvaluationResult NewArrayInit(NewArrayExpression expression) => GuardedInvocation(expression, NewArrayInitCore);
		EvaluationResult NewArrayInitCore(NewArrayExpression expression) {
			var result = Array.CreateInstance(expression.Type.GetElementType(), expression.Expressions.Count);
			for (var i = 0; i != result.Length; ++i)
				result.SetValue(Evaluate(expression.Expressions[i]).Result, i);
			return Success(expression.Type, result);
		}

		EvaluationResult Quote(UnaryExpression expression) => Success(expression.Type, expression.Operand);

		EvaluationResult Invoke(InvocationExpression expression) {
			var target = Evaluate(expression.Expression).Result as Delegate;
			return EvaluateAll(expression.Arguments)
				.Then<object[]>(arguments => {
					try {
						return Success(expression.Type, target.DynamicInvoke(arguments));
					} catch (TargetInvocationException e) {
						return Failure(expression, e.InnerException);
					}
				});
		}

		EvaluationResult AndAlso(BinaryExpression expression) =>
			Evaluate(expression.Left)
				.Then<bool>(leftResult => leftResult ? Evaluate(expression.Right) : Success(typeof(bool), false));

		EvaluationResult Parameter(ParameterExpression expression) => Success(expression.Type, parameters[expression]);

		ExpressionEvaluatorContext Rebind(Expression newContext) =>
			new(newContext, ExpressionEvaluatorParameters.Empty, Unsupported) {
				NullSubexpression = NullSubexpression
			};

		object ChangeType(object value, Type to) => ObjectConverter.ChangeType(value, to);

		static EvaluationResult Success(Type type, object value) => EvaluationResult.Success(type, value);
		static EvaluationResult Failure(Expression expression, Exception e) => EvaluationResult.Failure(expression, e);

		static EvaluationResult GuardedInvocation(Expression expression, Func<EvaluationResult> action, Action @finally) {
			try {
				return action();
			} catch (TargetInvocationException e) {
				return Failure(expression, e.InnerException);
			} finally { @finally(); }
		}
		
		static EvaluationResult GuardedInvocation<T>(T expression, Func<T, EvaluationResult> action) where T : Expression {
			try {
				return action(expression);
			} catch (TargetInvocationException e) {
				return Failure(expression, e.InnerException);
			}
		}
	}
}
