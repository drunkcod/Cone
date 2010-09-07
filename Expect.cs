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
        static readonly Expression BoxedTrue = Expression.TypeAs(Expression.Constant(true), typeof(object));

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
