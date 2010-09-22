using System;
using System.Linq.Expressions;

namespace Cone
{
    public class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        static readonly ExpressionFormatter ExpressionFormatter = new ExpressionFormatter();
        static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();

        static IExpect From(Expression body) {
            if(body.NodeType == ExpressionType.Not)
                return new NotExpect(From(((UnaryExpression)body).Operand));

            if (SupportedExpressionType(body.NodeType))
                return Lambda(body);
            throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
        }

        static bool SupportedExpressionType(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Call: return true;
                case ExpressionType.Constant: return true;
                case ExpressionType.Equal: return true;
                case ExpressionType.NotEqual: return true;
                case ExpressionType.GreaterThan: return true;
                case ExpressionType.GreaterThanOrEqual: return true;
                case ExpressionType.LessThan: return true;
                case ExpressionType.LessThanOrEqual: return true;
                case ExpressionType.MemberAccess: return true;
                case ExpressionType.TypeIs: return true;
            }
            return false;
        }

        public static object That(Expression<Func<bool>> expr) {
            return Check(From(expr.Body));
        }

        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return (TException)Check(new ExceptionExpect(expr, typeof(TException)));
        }

        static object Check(IExpect expect) {
            if (!expect.Check())
                ExpectationFailed(expect.FormatExpression(ExpressionFormatter) + "\n" + expect.FormatMessage(ParameterFormatter));
            return expect.Actual;
        }
        
        static Expect Lambda(Expression body) {
            var binary = body as BinaryExpression;
            if (binary != null)
                return BinaryExpect.From(binary);
            return Expect.From(body);
        }
    }
}
