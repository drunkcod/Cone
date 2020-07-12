using System;
using System.Linq.Expressions;
using System.Reflection;
using CheckThat.Formatting;
using CheckThat.Internals;

namespace CheckThat.Expectations
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

        public virtual ConeMessage FormatMessage(IFormatter<object> formatter) => MessageFormat(FormatActual(formatter), FormatExpected(formatter));

		public virtual string FormatActual(IFormatter<object> formatter) =>
			Actual.ToString(formatter);

        public string FormatExpected(IFormatter<object> formatter) =>
			Expected.ToString(formatter);

        public CheckResult Check() => 
			new CheckResult(CheckCore(), Maybe.Some(ActualValue), Maybe.Some(ExpectedValue));

        public virtual object ActualValue => actual.Value;
		public bool ExpectsNull => ExpectedValue == ExpectedNull.Value;
        public object ExpectedValue => Expected.Value; 

        public virtual ConeMessage MessageFormat(string actual, string expected) => ExpectMessages.EqualFormat(actual, expected);
		
        IExpectValue Actual => actual;
        protected virtual IExpectValue Expected => ExpectValue.True;

        protected virtual bool CheckCore() =>
			ActualValue != null
			? ActualValue.Equals(ExpectedValue)
            : ExpectedValue.Equals(ActualValue);
    }

    class ConversionExpect : BooleanExpect
    {
        readonly MethodInfo conversion;

        public ConversionExpect(Expression body, object actual, MethodInfo conversion) : base(body, new ExpectValue(actual)) {
            this.conversion = conversion;
        }

        protected override bool CheckCore() =>
			ActualValue != null
			? conversion.Invoke(null, new []{ ActualValue }).Equals(ExpectedValue)
            : ExpectedValue.Equals(ActualValue);
    }
}
