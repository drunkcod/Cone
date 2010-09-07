using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public interface IExpect
    {
        object Check(Action<string> onCheckFailed, ExpressionFormatter formatter);
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

        public object Check(Action<string> onCheckFailed, ExpressionFormatter formatter) {
            CheckCore(onCheckFailed, formatter);
            return actual;
        }

        protected abstract void CheckCore(Action<string> onCheckFailed, ExpressionFormatter formatter);
    }

    public class Expect : ExpectBase
    {
        public const string FormatExpression = "  {0} failed";

        static readonly ConstructorInfo ExpectCtor = typeof(Expect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object), typeof(bool) });

        readonly bool outcome;

        public static Expect From(Expression body, bool outcome) {
            return From(ExpectCtor, body, body, Expression.Constant(true), outcome);
        }

        internal    static Expect From(ConstructorInfo ctor, Expression body, Expression left, Expression right, bool outcome) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(ctor,
                        Expression.Constant(body),
                        Box(left),
                        Box(right),
                        Expression.Constant(outcome)))
                .Compile()();
        }

        static Expression Box(Expression expression) { return Expression.TypeAs(expression, typeof(object)); }

        public Expect(Expression body, object actual, object expected, bool outcome)
            : base(body, actual, expected) {
            this.outcome = outcome;
        }

        protected override void CheckCore(Action<string> onCheckFailed, ExpressionFormatter formatter) {
            if (expected.Equals(actual) != outcome)
                onCheckFailed(Format(formatter));
        }

        protected virtual string Format(ExpressionFormatter formatter) {
            return string.Format(FormatExpression, formatter.Format(body));
        }
    }
}
