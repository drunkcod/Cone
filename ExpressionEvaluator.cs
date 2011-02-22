using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExpressionEvaluator
    {
        public static T Evaluate<T>(Expression<Func<T>> lambda) { return EvaluateAs<T>(lambda.Body); }

        public static T EvaluateAs<T>(Expression body) {
            switch(body.NodeType) {
                case ExpressionType.Constant: return (T)(body as ConstantExpression).Value;
                default: return ExecuteAs<T>(body);
            }
        }

        static T ExecuteAs<T>(Expression body) { return body.CastTo<T>().Execute<T>(); }
    }
}
