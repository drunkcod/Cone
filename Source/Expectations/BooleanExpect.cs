using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
{
    public class BooleanExpect : IExpect
    {
        readonly protected Expression body;
        readonly protected object actual;

        public BooleanExpect(Expression body, object actual) {
            this.body = body;
            this.actual = actual;
        }

        public virtual string FormatExpression(IFormatter<Expression> formatter){ return formatter.Format(body); }
        public virtual string FormatExpected(IFormatter<object> formatter) { return formatter.Format(Expected); }
        public virtual string FormatMessage(IFormatter<object> formatter) { 
            return string.Format(MessageFormat, formatter.Format(actual), FormatExpected(formatter));
        }

        public ExpectResult Check() {
            return new ExpectResult {
                Actual = actual, 
                Success = CheckCore()
            };
        }

        public virtual object Actual { get { return actual; } }
        public virtual object Expected { get { return true; } }

        public virtual string MessageFormat { get { return ExpectMessages.EqualFormat; } }
        
        protected virtual bool CheckCore() {
            if(actual != null)
                return actual.Equals(Expected);
            return Expected.Equals(actual);
        }
    }

    class ConversionExpect : BooleanExpect
    {
        readonly MethodInfo conversion;

        public ConversionExpect(Expression body, object actual, MethodInfo conversion) : base(body, actual) {
            this.conversion = conversion;
        }

        protected override bool CheckCore() {
            if(actual != null)
                return conversion.Invoke(null, new[]{ actual }).Equals(Expected);
            return Expected.Equals(actual);
        }
    }

    public class Expect : BooleanExpect 
    {
        readonly object expected;

        public Expect(Expression body, object actual, object expected) : base(body, actual) {
            this.expected = expected ?? ExpectedNull.IsNull;
        }

        public override object Expected { get { return expected; } }
    }
}
