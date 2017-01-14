using System;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
{
    public interface IExpectValue
    {
        object Value { get; }
        string ToString(IFormatter<object> formatter);
    }

    public class ExpectValue : IExpectValue
    {
        readonly object value;

        public ExpectValue(object value) { this.value = value; }

        public object Value { get { return value; } }

        public string ToString(IFormatter<object> formatter) { return formatter.Format(Value); }
        public override string ToString() { return Value.ToString(); }
    }

    public class BooleanExpect : IExpect
    {
        protected readonly Expression body;
        readonly IExpectValue actual;

        public BooleanExpect(Expression body, IExpectValue actual) {
            if(actual == null) throw new ArgumentNullException("actual");
            this.body = body;
            this.actual = actual;
        }

        public virtual string FormatExpression(IFormatter<Expression> formatter) { 
			return formatter.Format(body); 
		}
        
        public virtual string FormatMessage(IFormatter<object> formatter) { 
            return string.Format(MessageFormat, FormatActual(formatter), FormatExpected(formatter));
        }

        public string FormatActual(IFormatter<object> formatter) {
            return Actual.ToString(formatter);
        }

        public string FormatExpected(IFormatter<object> formatter) {
            return Expected.ToString(formatter);
        }

        public CheckResult Check() {
            return new CheckResult(CheckCore(), Maybe.Some(ActualValue), Maybe.Some(ExpectedValue));
        }

        public virtual object ActualValue { get { return actual.Value; } }
        public object ExpectedValue { get { return Expected.Value; } }

        public virtual string MessageFormat { get { return ExpectMessages.EqualFormat; } }
        
        IExpectValue Actual { get { return actual; } }
        protected virtual IExpectValue Expected { get { return new ExpectValue(true); } }

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
                return conversion.Invoke(null, new[]{ ActualValue }).Equals(ExpectedValue);
            return ExpectedValue.Equals(ActualValue);
        }
    }

    public class Expect : BooleanExpect 
    {
        readonly IExpectValue expected;

        public Expect(Expression body, IExpectValue actual, IExpectValue expected) : base(body, actual) {            
            this.expected = expected.Value != null ? expected : new ExpectValue(ExpectedNull.IsNull);
        }

        protected override IExpectValue Expected { get { return expected; } }
    }
}
