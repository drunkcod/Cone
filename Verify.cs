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
            Expression body;
            Expect expect;
            bool outcome;

            public static BoundExpect From(Expression body) {
                switch (body.NodeType) {
                    case ExpressionType.Not:
                        var x = From(((UnaryExpression)body).Operand);
                        x.outcome = !x.outcome;
                        return x;

                    case ExpressionType.Call:
                        return new BoundExpect(Expect.FailFormat, body, true);

                    case ExpressionType.Constant: goto case ExpressionType.Call;

                     case ExpressionType.Equal:
                        return new BoundExpect(Expect.EqualFormat, Expect.EqualValuesFormat, (BinaryExpression)body, true);

                    case ExpressionType.NotEqual:
                        return new BoundExpect(Expect.NotEqualFormat, Expect.NotEqualValuesFormat, (BinaryExpression)body, false);
                }
                throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
            }

            BoundExpect(string format, Expression body, bool outcome) : this(body, outcome, Expect.Lambda(body, format)) { }
            BoundExpect(string format, string formatValues, BinaryExpression body, bool outcome) : this(body, outcome, Expect.Lambda(body, format, formatValues)) { }

            BoundExpect(Expression body, bool outcome, Expression<Func<Expect>> expect) {
                this.body = body;
                this.outcome = outcome;
                this.expect = expect.Compile()();
            }

            public bool Check() {
                return expect.Check() == outcome;
            }

            public string Format() { return expect.Format(body); }
        }

        public static void That(Expression<Func<bool>> expr) {
            var expect = BoundExpect.From(expr.Body);
            if(!expect.Check())
                ExpectationFailed(expect.Format());
        }
    }
}
