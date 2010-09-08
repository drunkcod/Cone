using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public interface IExpect
    {
        object Actual { get; }
        bool Check();
        string FormatExpression(IExpressionFormatter formatter);
        string FormatMessage(IExpressionFormatter formatter);
    }

    public abstract class ExpectBase : IExpect
    {
        readonly protected Expression body;
        readonly protected object expected;
        readonly protected object actual;

        protected ExpectBase(Expression body, object actual, object expected) {
            this.body = body;
            this.expected = expected;
            this.actual = actual;
        }

        public object Actual { get { return actual; } }
        public virtual string FormatExpression(IExpressionFormatter formatter){ return formatter.Format(body); }
        public virtual string FormatMessage(IExpressionFormatter formatter){ return string.Empty; }
        public abstract bool Check();
    }

    public class Expect : ExpectBase
    {
        public const string FormatExpression = "  {0} failed";

        static readonly ConstructorInfo ExpectCtor = typeof(Expect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });

        public static Expect From(Expression body) {
            return From(ExpectCtor, body, body, Expression.Constant(true));
        }

        internal static Expect From(ConstructorInfo ctor, Expression body, Expression left, Expression right) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(ctor,
                        Expression.Constant(body),
                        Box(left),
                        Box(right)))
                .Compile()();
        }

        static Expression Box(Expression expression) { return Expression.TypeAs(expression, typeof(object)); }

        public Expect(Expression body, object actual, object expected)
            : base(body, actual, expected) {
        }

        public override bool Check() {
            return expected.Equals(actual);
        }
    }
}
