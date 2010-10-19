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

    public class BooleanExpect : IExpect
    {
        readonly protected Expression body;
        readonly protected object actual;

        public BooleanExpect(Expression body, object actual) {
            this.body = body;
            this.actual = actual;
        }

        public virtual string FormatExpression(IFormatter<Expression> formatter){ return formatter.Format(body); }
        public virtual string FormatMessage(IFormatter<object> formatter) { 
            return string.Format(MessageFormat, formatter.Format(actual), formatter.Format(Expected));
        }

        public bool Check(out object actual) {
            actual = this.actual;
            return CheckCore();
        }

        public object Actual { get { return actual; } }
        public virtual object Expected { get { return true; } }

        protected virtual string MessageFormat { get { return string.Empty; } }
        
        protected virtual bool CheckCore() {
            return Expected.Equals(actual);
        }
    }

    public class Expect : BooleanExpect 
    {
        readonly object expected;

        public Expect(Expression body, object actual, object expected) : base(body, actual) {
            this.expected = expected ?? new ExpectedNull();
        }

        public override object Expected { get { return expected; } }

    }
}
