using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public class Expect
    {
        public const string FormatExpression = "  {0} failed";

        static readonly ConstructorInfo expector = typeof(Expect).GetConstructor(new[] { typeof(Expression), typeof(object) });
        
        protected readonly Expression body;
        protected readonly object actual;

        public static Expression<Func<Expect>> Lambda(Expression body) {
            var binary = body as BinaryExpression;
            if (binary != null)
                return BinaryExpect.Lambda(binary);
            return Expression.Lambda<Func<Expect>>(
                Expression.New(expector,
                        Expression.Constant(body),
                        Expression.TypeAs(body, typeof(object))));
        }

        public Expect(Expression body, object actual) {
            this.body = body;
            this.actual = actual;
        }

        public bool Check() { 
            return Expected.Equals(actual);
        }

        public virtual string Format(ExpressionFormatter formatter) {
            return string.Format(FormatExpression, formatter.Format(body));
        }

        public virtual string Format(params string[] args) {
            return string.Format(FormatExpression, args);
        }

        protected virtual object Expected { get { return true; } }
    }
}
