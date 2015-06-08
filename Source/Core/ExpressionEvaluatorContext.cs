using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Reflection;

namespace Cone.Core
{
	class ExpressionEvaluatorContext 
	{
		readonly Expression context;

		public Func<Expression,EvaluationResult> Unsupported;
		public Func<Expression, Expression, EvaluationResult> NullSubexpression;

		public ExpressionEvaluatorContext(Expression context) {
			this.context = context;
		}

		public EvaluationResult Evaluate(Expression body) {
			switch(body.NodeType) {
				case ExpressionType.Lambda: return EvaluateLambda(body);
				case ExpressionType.ArrayIndex: return EvaluateArrayIndex(body);
				case ExpressionType.Call: return EvaluateCall(body);
				case ExpressionType.Constant: return Success(body.Type, ((ConstantExpression)body).Value);
				case ExpressionType.Convert: return EvaluateConvert(body);
				case ExpressionType.Equal: goto case ExpressionType.NotEqual;
				case ExpressionType.NotEqual: return EvaluateBinary(body);
				case ExpressionType.MemberAccess: return EvaluateMemberAccess(body);
				case ExpressionType.New: return EvaluateNew(body);
				case ExpressionType.NewArrayInit: return EvaluateNewArrayInit(body);
				case ExpressionType.Quote: return EvaluateQuote(body);
				case ExpressionType.Invoke: return EvaluateInvoke(body);
				case ExpressionType.AndAlso: return EvaluateAndAlso(body);
				default: return Unsupported(body);
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
			var result = Array.ConvertAll(evald, x => x.Result);
			return Success(typeof(object[]), result);
		}


		EvaluationResult EvaluateLambda(Expression expression) { return EvaluateLambda((LambdaExpression)expression); }
		EvaluationResult EvaluateLambda(LambdaExpression expression) {
			if(expression == context && expression.Parameters.Count == 0)
				return Evaluate(expression.Body);
			return Success(expression.Type, expression.Compile());
		}

		EvaluationResult EvaluateArrayIndex(Expression expression) {
			var rank1 = expression as BinaryExpression;
			if(rank1 != null)
				return EvaluateArrayIndex1(rank1);
			return Unsupported(expression);
		}

		EvaluationResult EvaluateArrayIndex1(BinaryExpression rank1) {
			var array = (Array)Evaluate(rank1.Left).Result;
			var index = (int)Evaluate(rank1.Right).Result;
			return Success(rank1.Type, array.GetValue(index));
		}

		EvaluationResult EvaluateBinary(Expression expression) { return EvaluateBinary((BinaryExpression)expression); }
		EvaluationResult EvaluateBinary(BinaryExpression binary) {
			return Evaluate(binary.Left)
				.Then<object>(left => Evaluate(binary.Right)
				.Then<object>(right => EvaluateBinary(binary, left, right)));
		}

		EvaluationResult EvaluateBinary(BinaryExpression binary, object left, object right) {
			var op = binary.Method;
			if(op != null)
				return Success(op.ReturnType, op.Invoke(null, new[]{ left, right }));
			switch(binary.NodeType) {
				case ExpressionType.Equal: return Success(typeof(bool), Object.Equals(left, right));
				case ExpressionType.NotEqual: return Success(typeof(bool), !Object.Equals(left, right));
				default: return Unsupported(binary);
			}
		}

		EvaluationResult EvaluateCall(Expression expression) { return EvaluateCall((MethodCallExpression)expression); }
		EvaluationResult EvaluateCall(MethodCallExpression expression) {
			return EvaluateAsTarget(expression.Object).Then<object>(target =>
				EvaluateAll(expression.Arguments).Then<object[]>(input => {
					var method = expression.Method;
					return GuardedInvocation(expression,
						() => Success(method.ReturnType, method.Invoke(target, input)),
						() => AssignOutParameters(expression.Arguments, input, method.GetParameters()));
				}));
		}

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

		EvaluationResult EvaluateConvert(Expression expression) { return EvaluateConvert((UnaryExpression)expression); }
		EvaluationResult EvaluateConvert(UnaryExpression expression) {
			return Evaluate(expression.Operand).Then<object>(value => {
				var convertMethod = expression.Method;
				if(convertMethod != null && convertMethod.IsStatic) {
					return GuardedInvocation(expression, () => Success(convertMethod.ReturnType, convertMethod.Invoke(null, new[] { value })));
				}
				return Success(expression.Type, ChangeType(value, expression.Type));
			});
		}

		EvaluationResult EvaluateMemberAccess(Expression expression) { return EvaluateMemberAccess((MemberExpression)expression); }
		EvaluationResult EvaluateMemberAccess(MemberExpression expression) {
			return GuardedInvocation(expression, () =>
				EvaluateAsTarget(expression.Expression)
				.Then<object>(x => Success(expression.Type, expression.Member.GetValue(x))));
		}

		EvaluationResult EvaluateNew(Expression expression) { return EvaluateNew((NewExpression)expression); }
		EvaluationResult EvaluateNew(NewExpression expression) {
			return GuardedInvocation(expression, () => {
				var args = EvaluateAll(expression.Arguments).Result as object[];
				if(expression.Constructor != null)
					return Success(expression.Type, expression.Constructor.Invoke(args));
				return Success(expression.Type, Activator.CreateInstance(expression.Type, args));
			});
		}

		EvaluationResult EvaluateNewArrayInit(Expression expression) { return EvaluateNewArrayInit((NewArrayExpression)expression);}
		EvaluationResult EvaluateNewArrayInit(NewArrayExpression expression) {
			return GuardedInvocation(expression, () => {
				var result = Array.CreateInstance(expression.Type.GetElementType(), expression.Expressions.Count);
				for(var i = 0; i != result.Length; ++i)
					result.SetValue(Evaluate(expression.Expressions[i]).Result, i);
				return Success(expression.Type, result);
			});
		}

		EvaluationResult GuardedInvocation(Expression expression, Func<EvaluationResult> action) { return GuardedInvocation(expression, action, () => {}); }
		EvaluationResult GuardedInvocation(Expression expression, Func<EvaluationResult> action, Action @finally) {
			try {
				return action();
			} catch(TargetInvocationException e) {
				return Failure(expression, e.InnerException);
			} finally { @finally(); }
		}

		EvaluationResult EvaluateQuote(Expression expression) { return EvaluateQuote((UnaryExpression)expression); }
		EvaluationResult EvaluateQuote(UnaryExpression expression) {
			return Success(expression.Type, expression.Operand);
		}

		EvaluationResult EvaluateInvoke(Expression expression) { return EvaluateInvoke((InvocationExpression)expression); }
		EvaluationResult EvaluateInvoke(InvocationExpression expression) {
			var target = Evaluate(expression.Expression).Result as Delegate;
			return EvaluateAll(expression.Arguments)
				.Then<object[]>(arguments => GuardedInvocation(expression, () => Success(expression.Type, target.DynamicInvoke(arguments))));
		}

		EvaluationResult EvaluateAndAlso(Expression expression) { return EvaluateAndAlso((BinaryExpression)expression); }
		EvaluationResult EvaluateAndAlso(BinaryExpression expression) {
			return Evaluate(expression.Left)
				.Then<bool>(leftResult => leftResult ? Evaluate(expression.Right) : EvaluationResult.Success(typeof(bool), false));
		}

		ExpressionEvaluatorContext Rebind(Expression newContext) {
			return new ExpressionEvaluatorContext(newContext) {
				Unsupported = Unsupported,
				NullSubexpression = NullSubexpression
			};
		}

		object ChangeType(object value, Type to) {
			return ObjectConverter.ChangeType(value, to);
		}

		EvaluationResult Success(Type type, object value){ return EvaluationResult.Success(type, value); }
		EvaluationResult Failure(Expression expression, Exception e){ return EvaluationResult.Failure(expression, e); } 
	}
}
