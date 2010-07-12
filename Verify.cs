using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public static class Verify
    {
        internal static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        
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
            static readonly ConstructorInfo expector = typeof(Expect).GetConstructor(new[] { typeof(object), typeof(object), typeof(string), typeof(bool) });
            Expression actual, expected;
            Lazy<Expect> expect;

            public static BoundExpect From(Expression body) {
                if (body.NodeType == ExpressionType.Call)
                    return From((MethodCallExpression)body);
                else
                    return From((BinaryExpression)body);
            }

            public static BoundExpect From(MethodCallExpression call){
                return new BoundExpect(call.NodeType, 
                    call, 
                    Expression.Constant(true),
                    true);
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
                expect = new Lazy<Expect>(Expression.Lambda<Func<Expect>>(
                        Expression.New(expector,
                            Expression.TypeAs(actual, typeof(object)),
                            Expression.TypeAs(expected, typeof(object)),
                            Expression.Constant(format), Expression.Constant(outcome)))
                    .Compile());
            }

            public bool Check() {
                return expect.Value.Check();
            }

            public string Format() { return expect.Value.Format(actual, expected); }

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
