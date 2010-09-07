using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public class Expect
    {
        public const string FormatExpression = "  {0} failed";

        static readonly ConstructorInfo expector = typeof(Expect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });

        protected readonly Expression body;
        protected readonly object actual;
        protected readonly object expected;

        public static Expression<Func<Expect>> Lambda(Expression body) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(expector,
                        Expression.Constant(body),
                        Expression.TypeAs(body, typeof(object)), 
                        Expression.TypeAs(Expression.Constant(true), typeof(object))));
        }
        
        public Expect(Expression body, object actual, object expected) {
            this.body = body;
            this.actual = actual;
            this.expected = expected;
        }

        public void Check(bool outcome, Action<string> onCheckFailed, ExpressionFormatter formatter) {
            if (Expected.Equals(actual) != outcome)
                onCheckFailed(Format(formatter));
        }

        public virtual string Format(ExpressionFormatter formatter) {
            return string.Format(FormatExpression, formatter.Format(body));
        }

        public virtual string Format(params string[] args) {
            return string.Format(FormatExpression, args);
        }

        protected virtual object Expected { get { return expected; } }
    }
}
