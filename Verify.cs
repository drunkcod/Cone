using System;
using System.Linq.Expressions;

namespace Cone
{
    public class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        static Expect From(Expression body, bool outcome) {
            switch (body.NodeType) {
                case ExpressionType.Not:
                    return From(((UnaryExpression)body).Operand, !outcome);
                default:
                    if (UnsupportedExpressionType(body.NodeType))
                        throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
                    return Lambda(body, outcome);
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
            Check(From(expr.Body, true));
        }

        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return (TException)Check(new ExceptionExpect(expr, typeof(TException)));
        }

        static object Check(IExpect expect) {
            return expect.Check(ExpectationFailed, Formatter);
        }
        
        static Expect Lambda(Expression body, bool outcome) {
            outcome &= body.NodeType != ExpressionType.NotEqual;
            var binary = body as BinaryExpression;
            if (binary != null)
                return BinaryExpect.From(binary, outcome);
            return Expect.From(body, outcome);
        }
    }
}
