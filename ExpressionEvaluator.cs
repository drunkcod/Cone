using System.Linq.Expressions;

namespace Cone
{
    class ExpressionEvaluator
    {
        public static T EvaluateAs<T>(Expression body) {
            switch(body.NodeType) {
                case ExpressionType.Constant: return (T)(body as ConstantExpression).Value;
                default: return body.ExecuteAs<T>();
            }
        }
    }
}
