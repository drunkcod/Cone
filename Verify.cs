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
            bool outcome;

            public static BoundExpect From(Expression body) {
                switch (body.NodeType) {
                    case ExpressionType.Not:
                        var x = From(((UnaryExpression)body).Operand);
                        x.outcome = !x.outcome;
                        return x;

                    case ExpressionType.Call: return new BoundExpect(body);
                    case ExpressionType.Constant: return new BoundExpect(body);
                    case ExpressionType.Equal: return new BoundExpect(body);
                    case ExpressionType.NotEqual: return new BoundExpect(body);
                }
                throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
            }

            BoundExpect(Expression body){
                this.body = body;
                this.outcome = body.NodeType != ExpressionType.NotEqual;
            }

            public void Check(Action<string> onError) {
                var expect = Expect.Lambda(body).Compile()();
                if(expect.Check() != outcome)
                    onError(expect.Format());
            }
        }

        public static void That(Expression<Func<bool>> expr) {
            var expect = BoundExpect.From(expr.Body);
            expect.Check(ExpectationFailed);
        }
    }
}
