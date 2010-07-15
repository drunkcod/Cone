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
                switch (body.NodeType) {
                    case ExpressionType.Not:
                        var x = From(((UnaryExpression)body).Operand);
                        x.outcome = !x.outcome;
                        return x;

                    case ExpressionType.Call:
                        return new BoundExpect(Expect.FailFormat,
                            body,
                            Expression.Constant(true),
                            true);

                    case ExpressionType.Constant:
                        var constant = (ConstantExpression)body;
                        return new BoundExpect(body, Expression.Constant(true), true, new Expect((bool)constant.Value, true, Expect.FailFormat));

                    case ExpressionType.Equal:
                        return FromBinary(Expect.EqualFormat, (BinaryExpression)body);

                    case ExpressionType.NotEqual:
                        return FromBinary(Expect.NotEqualFormat, (BinaryExpression)body);
                }
                throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
            }

            public static BoundExpect FromBinary(string format, BinaryExpression body) {
                return new BoundExpect(format, 
                    body.Left, 
                    body.Right,
                    body.NodeType == ExpressionType.Equal);
            }

            BoundExpect(string format, Expression actual, Expression expected, bool outcome) {
                this.actual = actual;
                this.expected = expected;
                this.outcome = outcome;
                this.expect = Expression.Lambda<Func<Expect>>(
                        Expression.New(expector,
                            Expression.TypeAs(actual, typeof(object)),
                            Expression.TypeAs(expected, typeof(object)),
                            Expression.Constant(format)))
                    .Compile()();
            }

            BoundExpect(Expression actual, Expression expected, bool outcome, Expect expect) {
                this.actual = actual;
                this.expected = expected;
                this.outcome = outcome;
                this.expect = expect;
            }

            public bool Check() {
                return expect.Check() == outcome;
            }

            public string Format() { return expect.Format(actual, expected); }
        }

        public static void That(Expression<Func<bool>> expr) {
            var expect = BoundExpect.From(expr.Body);
            if(!expect.Check())
                ExpectationFailed(expect.Format());
        }
    }
}
