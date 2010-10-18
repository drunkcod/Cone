using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Expectations
{
    public interface IExpect
    {
        bool Check(out object actual);
        string FormatExpression(IFormatter<Expression> formatter);
        string FormatMessage(IFormatter<object> formatter);
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

        public virtual string FormatExpression(IFormatter<Expression> formatter){ return formatter.Format(body); }
        public virtual string FormatMessage(IFormatter<object> formatter){ return string.Empty; }
        public bool Check(out object actual) {
            actual = this.actual;
            return CheckCore();
        }

        protected abstract bool CheckCore();
    }

    public class Expect : ExpectBase
    {
        public Expect(Expression body, object actual, object expected)
            : base(body, actual, expected) {
        }

        protected override bool CheckCore() {
            return expected.Equals(actual);
        }
    }
}
