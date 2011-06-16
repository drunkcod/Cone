using System;
using System.Linq.Expressions;
using System.Collections.Generic;

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
       
        public EvaluationResult Evaluate(Expression body, Expression context) { 
            return Evaluate(body, context, x => { throw new ExceptionExpressionException(x.Expression, context, x.Exception); });
        }

        public EvaluationResult Evaluate(Expression body, Expression context, Func<EvaluationResult, EvaluationResult> onError) {
            var result = CreateContext(context).Evaluate(body);
            if(result.IsError)
                return onError(result);
            return result;
        }

        public EvaluationResult EvaluateAsTarget(Expression expression, Expression context) {
            return CreateContext(context).EvaluateAsTarget(expression);
        }

        public EvaluationResult EvaluateAll(ICollection<Expression> expressions, Expression context) {
            return CreateContext(context).EvaluateAll(expressions);
        }

        ExpressionEvaluatorContext CreateContext(Expression context) {
            return new ExpressionEvaluatorContext(context) {
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
