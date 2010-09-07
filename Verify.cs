using System;
using System.Linq.Expressions;

namespace Cone
{
    public class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        static IExpect From(Expression body) {
            switch (body.NodeType) {
                case ExpressionType.Not:
                    return new NotExpect(From(((UnaryExpression)body).Operand));
                default:
                    if (UnsupportedExpressionType(body.NodeType))
                        throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
                    return Lambda(body);
            }
        }

        static bool UnsupportedExpressionType(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Call: return false;
                case ExpressionType.Constant: return false;
                case ExpressionType.Equal: return false;
                case ExpressionType.NotEqual: return false;
                case ExpressionType.MemberAccess: return false;
            }
            return true;
        }

        public static void That(Expression<Func<bool>> expr) {
            Check(From(expr.Body));
        }

        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return (TException)Check(new ExceptionExpect(expr, typeof(TException)));
        }

        static object Check(IExpect expect) {
            if (!expect.Check())
                ExpectationFailed(expect.FormatBody(Formatter) + "\n" + expect.FormatValues(Formatter));
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
