using System;
using System.Linq.Expressions;

namespace Cone
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
            var result = EvaluateCore(body, CreateContext(context));
            if(result.IsError)
                return onError(result);
            return result;
        }

        public EvaluationResult EvaluateAsTarget(Expression expression, Expression context) {
            return CreateContext(context).EvaluateAsTarget(expression);
        }

        ExpressionEvaluatorContext CreateContext(Expression context) {
            return new ExpressionEvaluatorContext(context) {
                Unsupported = Unsupported,
                NullSubexpression = NullSubexpression
            };
        }

        EvaluationResult EvaluateCore(Expression body, ExpressionEvaluatorContext context) {
            return context.Evaluate(body);
        }

        EvaluationResult EvaluateUnsupported(Expression expression) {
            try {
                return EvaluationResult.Success(Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile()());
            } catch(Exception e) {
                return EvaluationResult.Failure(expression, e);
            }
        }

        EvaluationResult EvaluateNullSubexpression(Expression expression, Expression context) {
            return EvaluationResult.Failure(expression, new NullSubexpressionException(context, expression));
        }
    }
}
