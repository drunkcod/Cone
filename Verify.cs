using System;
using System.Linq.Expressions;

namespace Cone
{
    public static class Verify
    {
        internal static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };

        public static void That(Expression<Func<bool>> expr) {
            var body = expr.Body as BinaryExpression;
            var actual = body.Left;
            var expected = body.Right;

            var types = new[] { actual.Type, expected.Type };

            var expect = Expression.Lambda<Func<Expect>>(
                    Expression.Call(typeof(Expect), "New", types, actual, expected))
                .Compile()();

            switch (body.NodeType) {
                case ExpressionType.NotEqual:
                    if (expect.Equal())
                        ExpectationFailed(expect.FormatNotEqual(Format(actual), Format(expected)));
                    break;
                case ExpressionType.Equal:
                    if (!expect.Equal())
                        ExpectationFailed(expect.FormatEqual(Format(actual), Format(expected)));
                    break;
                default: throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
            }
        }

        static string Format(Expression expr) {
            switch (expr.NodeType) {
                case ExpressionType.ArrayLength:
                    var unary = (UnaryExpression)expr;
                    return Format(unary.Operand) + ".Length";
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expr;
                    if (member.Expression.NodeType == ExpressionType.Constant)
                        return member.Member.Name;
                    return Format(member.Expression) + "." + member.Member.Name;
            }
            return expr.ToString();
        }
    }
}
