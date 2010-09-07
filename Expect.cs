using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public interface IExpect
    {
        object Check(Action<string> onCheckFailed, ExpressionFormatter formatter);
    }

    public class Expect : IExpect
    {
        public const string FormatExpression = "  {0} failed";

        static readonly ConstructorInfo ExpectCtor = typeof(Expect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object), typeof(bool) });
        static readonly Expression BoxedTrue = Expression.TypeAs(Expression.Constant(true), typeof(object));

        protected readonly Expression body;
        protected readonly object actual;
        protected readonly object expected;
        readonly bool outcome;

        public static Expect From(Expression body, bool outcome) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(ExpectCtor,
                        Expression.Constant(body),
                        Expression.TypeAs(body, typeof(object)), 
                        BoxedTrue,
                        Expression.Constant(outcome)))
                .Compile()();
        }
        
        public Expect(Expression body, object actual, object expected, bool outcome) {
            this.body = body;
            this.actual = actual;
            this.expected = expected;
            this.outcome = outcome;
        }

        public object Check(Action<string> onCheckFailed, ExpressionFormatter formatter) {
            if (Expected.Equals(actual) != outcome)
                onCheckFailed(Format(formatter));
            return actual;
        }

        string Format(ExpressionFormatter formatter) {
            return string.Format(FormatExpression, formatter.Format(body));
        }

        protected virtual object Expected { get { return expected; } }
    }
}
