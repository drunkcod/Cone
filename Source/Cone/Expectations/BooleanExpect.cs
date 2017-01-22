using System;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
{
    public class BooleanExpect : IExpect
    {
        protected readonly Expression body;
        readonly IExpectValue actual;

        public BooleanExpect(Expression body, IExpectValue actual) {
            if(actual == null) throw new ArgumentNullException("actual");
            this.body = body;
            this.actual = actual;
        }

        public virtual string FormatExpression(IFormatter<Expression> formatter) =>
			formatter.Format(body); 

        public virtual ConeMessage FormatMessage(IFormatter<object> formatter) => 
			ConeMessage.Parse(string.Format(MessageFormat, FormatActual(formatter), FormatExpected(formatter)));

		public string FormatActual(IFormatter<object> formatter) =>
			Actual.ToString(formatter);

        public string FormatExpected(IFormatter<object> formatter) =>
			Expected.ToString(formatter);

        public CheckResult Check() => 
			new CheckResult(CheckCore(), Maybe.Some(ActualValue), Maybe.Some(ExpectedValue));

        public virtual object ActualValue => actual.Value;
        public object ExpectedValue => Expected.Value; 

        public virtual string MessageFormat => ExpectMessages.EqualFormat;
        
        IExpectValue Actual => actual;
        protected virtual IExpectValue Expected => ExpectValue.True;

        protected virtual bool CheckCore() {
            if(ActualValue != null)
                return ActualValue.Equals(ExpectedValue);
            return ExpectedValue.Equals(ActualValue);
        }
    }

    class ConversionExpect : BooleanExpect
    {
        readonly MethodInfo conversion;

        public ConversionExpect(Expression body, object actual, MethodInfo conversion) : base(body, new ExpectValue(actual)) {
            this.conversion = conversion;
        }

        protected override bool CheckCore() {
            if(ActualValue != null)
                return conversion.Invoke(null, new []{ ActualValue }).Equals(ExpectedValue);
            return ExpectedValue.Equals(ActualValue);
        }
    }

    public class Expect : BooleanExpect 
    {
        readonly IExpectValue expected;

        public Expect(Expression body, IExpectValue actual, IExpectValue expected) : base(body, actual) {            
            this.expected = expected.Value != null ? expected : ExpectValue.Null;
        }

        protected override IExpectValue Expected => expected;
    }
}
