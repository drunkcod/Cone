using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cone.Core
{
	public class ExpressionEvaluator
    {
        public Func<Expression,EvaluationResult> Unsupported;
        public Func<Expression, Expression, EvaluationResult> NullSubexpression;

        public ExpressionEvaluator() {
            Unsupported = EvaluateUnsupported;
            NullSubexpression = EvaluateNullSubexpression;
        }

        public Expression Unwrap(Expression expression) {
            if(expression.NodeType == ExpressionType.Convert) 
                return (expression as UnaryExpression).Operand;
            return expression;
        }
       
        public EvaluationResult Evaluate(Expression body, Expression context, ExpressionEvaluatorParameters parameters = null) { 
            return Evaluate(body, context, parameters, x => { throw new ExceptionExpressionException(x.Expression, context, x.Exception); });
        }

        public EvaluationResult Evaluate(Expression body, Expression context, ExpressionEvaluatorParameters parameters, Func<EvaluationResult, EvaluationResult> onError) {
            var result = CreateContext(context, parameters ?? ExpressionEvaluatorParameters.Empty).Evaluate(body);
            if(result.IsError)
                return onError(result);
            return result;
        }

        public EvaluationResult EvaluateAsTarget(Expression expression, Expression context, ExpressionEvaluatorParameters contextParameters) {
            return CreateContext(context, contextParameters).EvaluateAsTarget(expression);
        }

        public EvaluationResult EvaluateAll(ICollection<Expression> expressions, Expression context) {
            return CreateContext(context, ExpressionEvaluatorParameters.Empty).EvaluateAll(expressions);
        }

        ExpressionEvaluatorContext CreateContext(Expression context, ExpressionEvaluatorParameters parameters) {
            return new ExpressionEvaluatorContext(context, parameters) {
                Unsupported = Unsupported,
                NullSubexpression = NullSubexpression
            };
        }

        EvaluationResult EvaluateUnsupported(Expression expression) {
            try {
                return EvaluationResult.Success(expression.Type, Expression.Lambda<Func<object>>(expression.Box()).Compile()());
            } catch(Exception e) {
                return EvaluationResult.Failure(expression, e);
            }
        }

        EvaluationResult EvaluateNullSubexpression(Expression expression, Expression context) {
            return EvaluationResult.Failure(expression, new NullSubexpressionException(context, expression));
        }
    }
}
