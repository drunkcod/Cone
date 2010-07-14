using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public static class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        
        class Lazy<T>
        {
            bool forced;
            object value;

            public Lazy(Func<T> func) {
                value = func;
            }

            public T Value { get { return forced ? (T)value : Force(); } }

            T Force() {
                forced = true;
                var tmp = ((Func<T>)value)();
                value = tmp;
                return tmp;
            }
        }

        struct BoundExpect
        {
            static readonly ConstructorInfo expector = typeof(Expect).GetConstructor(new[] { typeof(object), typeof(object), typeof(string) });
            Expression actual, expected;
            Expect expect;
            bool outcome;

            public static BoundExpect From(Expression body) {
                if (body.NodeType == ExpressionType.Not) {
                    var x = From(((UnaryExpression)body).Operand);
                    x.outcome = !x.outcome;
                    return x;
                }
                else if (body.NodeType == ExpressionType.Call)
                    return new BoundExpect(body.NodeType,
                        body,
                        Expression.Constant(true),
                        true);
                else
                    return From((BinaryExpression)body);
            }

            public static BoundExpect From(BinaryExpression body) {
                return new BoundExpect(body.NodeType, 
                    body.Left, 
                    body.Right,
                    body.NodeType == ExpressionType.Equal);
            }

            BoundExpect(ExpressionType nodeType, Expression actual, Expression expected, bool outcome) {
                var format = VerifyAndGetFormat(nodeType);
                this.actual = actual;
                this.expected = expected;
                this.outcome = outcome;
                expect = Expression.Lambda<Func<Expect>>(
                        Expression.New(expector,
                            Expression.TypeAs(actual, typeof(object)),
                            Expression.TypeAs(expected, typeof(object)),
                            Expression.Constant(format)))
                    .Compile()();
            }

            public bool Check() {
                return expect.Check() == outcome;
            }

            public string Format() { return expect.Format(actual, expected); }

            static string VerifyAndGetFormat(ExpressionType nodeType) {
                switch (nodeType) {
                    case ExpressionType.Call: return Expect.FailFormat;
                    case ExpressionType.Equal: return Expect.EqualFormat;
                    case ExpressionType.NotEqual: return Expect.NotEqualFormat;
                    default: throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", nodeType));
                }
            }
        }

        public static void That(Expression<Func<bool>> expr) {
            var expect = BoundExpect.From(expr.Body);
            if(!expect.Check())
                ExpectationFailed(expect.Format());
        }
    }
}
