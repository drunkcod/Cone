using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Reflection;
using System.Diagnostics;

namespace Cone.Core
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

		public EvaluationResult Evaluate(Expression body) {
			switch(body.NodeType) {
				case ExpressionType.Lambda: return Lambda((LambdaExpression)body);
				case ExpressionType.ArrayIndex: return ArrayIndex(body);
				case ExpressionType.ArrayLength: return ArrayLength(body);
				case ExpressionType.Call: return Call((MethodCallExpression)body);
				case ExpressionType.Constant: return Success(body.Type, ((ConstantExpression)body).Value);
				case ExpressionType.Convert: return Convert((UnaryExpression)body);
				case ExpressionType.Equal: goto case ExpressionType.NotEqual;
				case ExpressionType.NotEqual: return Binary((BinaryExpression)body);
				case ExpressionType.MemberAccess: return MemberAccess((MemberExpression)body);
				case ExpressionType.New: return New((NewExpression)body);
				case ExpressionType.NewArrayInit: return NewArrayInit((NewArrayExpression)body);
				case ExpressionType.Quote: return Quote((UnaryExpression)body);
				case ExpressionType.Invoke: return Invoke((InvocationExpression)body);
				case ExpressionType.AndAlso: return AndAlso((BinaryExpression)body);
				case ExpressionType.Parameter: return Parameter((ParameterExpression)body);
				default: return Unsupported(body, parameters);
			}
		}

		public EvaluationResult EvaluateAsTarget(Expression expression) {
			if(expression == null)
				return Success(null, null);
			return Evaluate(expression)
				.Then(result => result.IsNull ? NullSubexpression(expression, context) : result);
		}

		public EvaluationResult EvaluateAll(ICollection<Expression> expressions) {
			var evald = Array.ConvertAll(expressions.ToArray(), Evaluate);
			var fail = Array.FindIndex(evald, x => x.IsError);
			if(fail != -1)
				return evald[fail];
			return Success(typeof(object[]), Array.ConvertAll(evald, x => x.Result));
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
			EvaluateAsTarget(expression.Object).Then<object>(target =>
				EvaluateAll(expression.Arguments).Then<object[]>(input => {
					var method = expression.Method;
					return GuardedInvocation(expression,
						() => Success(method.ReturnType, method.Invoke(target, input)),
						() => AssignOutParameters(expression.Arguments, input, method.GetParameters()));
				}));

		void AssignOutParameters(IList<Expression> arguments, object[] results, ParameterInfo[] parameters) {
			if(results.Length == 0)
				return;
			for(int i = 0; i != parameters.Length; ++i)
				if(parameters[i].IsOut) {
					var member = (arguments[i] as MemberExpression);
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
			new ExpressionEvaluatorContext(newContext, ExpressionEvaluatorParameters.Empty, Unsupported) {
				NullSubexpression = NullSubexpression
			};

		object ChangeType(object value, Type to) => ObjectConverter.ChangeType(value, to);

		EvaluationResult Success(Type type, object value) => EvaluationResult.Success(type, value);
		EvaluationResult Failure(Expression expression, Exception e) => EvaluationResult.Failure(expression, e);

		EvaluationResult GuardedInvocation(Expression expression, Func<EvaluationResult> action, Action @finally) {
			try {
				return action();
			}
			catch (TargetInvocationException e) {
				return Failure(expression, e.InnerException);
			}
			finally { @finally(); }
		}

		EvaluationResult GuardedInvocation<T>(T expression, Func<T, EvaluationResult> action) where T : Expression {
			try {
				return action(expression);
			}
			catch (TargetInvocationException e) {
				return Failure(expression, e.InnerException);
			}
		}
	}
}