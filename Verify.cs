using System;
using System.Linq.Expressions;

namespace Cone
{
    public static class Verify
    {
        internal static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };

        struct BoundExpect
        {
            Expression actual, expected;
            Expect expect;
            string format;
            bool outcome;

            public static BoundExpect From(Expression body) {
                if (body.NodeType == ExpressionType.Call) {
                    return From((MethodCallExpression)body);
                } else {
                    return From((BinaryExpression)body);
                }
            }

            public static BoundExpect From(MethodCallExpression call){
                return new BoundExpect { 
                    actual = call, 
                    expected = Expression.Constant(true),
                    format = Expect.FailFormat,
                    outcome = true
                };
            }

            public static BoundExpect From(BinaryExpression body) {
                var expect = new BoundExpect { 
                    actual = body.Left, 
                    expected = body.Right,
                    format = Expect.EqualFormat,
                    outcome = true
                };
                switch (body.NodeType) {
                    case ExpressionType.Equal: break;
                    case ExpressionType.NotEqual:
                        expect.format = Expect.NotEqualFormat;
                        expect.outcome = false;
                        break;
                    default: throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
                }
                return expect;
            }

            public bool Check() {
                return Result().Check();
            }

            public string Format() { return Result().Format(actual, expected); }

            Expect Result() {
                if (expect != null)
                    return expect;
                var types = new[] { actual.Type, expected.Type };
                return expect = Expression.Lambda<Func<Expect>>(
                        Expression.New(typeof(Expect).GetConstructors()[0],
                            Expression.TypeAs(actual, typeof(object)), 
                            Expression.TypeAs(expected, typeof(object)), 
                            Expression.Constant(format), Expression.Constant(outcome)))
                    .Compile()();
            }
        }

        public static void That(Expression<Func<bool>> expr) {
            var expect = BoundExpect.From(expr.Body);
            if(!expect.Check())
                ExpectationFailed(expect.Format());
        }
    }
}
