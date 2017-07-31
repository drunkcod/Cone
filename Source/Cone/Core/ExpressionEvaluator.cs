using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Cone.Core
{
	public class ExpressionEvaluator
	{
		public Func<Expression, ExpressionEvaluatorParameters,EvaluationResult> Unsupported;
		public Func<Expression, Expression, EvaluationResult> NullSubexpression;

		public ExpressionEvaluator() {
			Unsupported = EvaluateUnsupported;
			NullSubexpression = EvaluateNullSubexpression;
		}

		public bool TryUnwrap(Expression expression, out Expression unwrapped) {
			if(expression.NodeType == ExpressionType.Convert) {
				unwrapped = (expression as UnaryExpression).Operand;
				return true;
			}
			unwrapped = null;
			return false;
		}

		public EvaluationResult Evaluate(Expression body, Expression context, ExpressionEvaluatorParameters parameters) => 
			Evaluate(body, context, parameters, x => { throw new ExceptionExpressionException(x.Expression, context, x.Exception); });

		public EvaluationResult Evaluate(Expression body, Expression context, ExpressionEvaluatorParameters parameters, Func<EvaluationResult, EvaluationResult> onError) {
			var result = CreateContext(context, parameters).Evaluate(body);
			return result.IsError ? onError(result) : result;
		}

		public EvaluationResult EvaluateAsTarget(Expression expression, Expression context, ExpressionEvaluatorParameters contextParameters) =>
			CreateContext(context, contextParameters).EvaluateAsTarget(expression);

		public EvaluationResult EvaluateAll(ICollection<Expression> expressions, Expression context) =>
			CreateContext(context, ExpressionEvaluatorParameters.Empty).EvaluateAll(expressions);

		ExpressionEvaluatorContext CreateContext(Expression context, ExpressionEvaluatorParameters parameters) =>
			new ExpressionEvaluatorContext(context, parameters, Unsupported) {
				NullSubexpression = NullSubexpression
			};

		EvaluationResult EvaluateUnsupported(Expression expression, ExpressionEvaluatorParameters parameters) {
			try {
				if(parameters != null) {
					var e = Expression.Lambda(expression, parameters.GetParameters());
					return EvaluationResult.Success(expression.Type, Expression.Lambda<Func<object>>(Expression.Invoke(e, parameters.Select(p => Expression.Constant(p.Value))).Box()).Compile()());
				}
				return EvaluationResult.Success(expression.Type, Expression.Lambda<Func<object>>(expression.Box()).Compile()());
			} catch(Exception e) {
				Console.Error.WriteLine(e.Message);
				return EvaluationResult.Failure(expression, e);
			}
		}

		EvaluationResult EvaluateNullSubexpression(Expression expression, Expression context) =>
			EvaluationResult.Failure(expression, new NullSubexpressionException(context, expression));
	}
}
