using Cone.Core;

namespace CheckThat.Expectations
{
	public interface IExpectValue
	{
		object Value { get; }
		string ToString(IFormatter<object> formatter);
	}

	public class ExpectValue : IExpectValue
	{
		public static readonly ExpectValue Null = new ExpectValue(ExpectedNull.Value);
		public static readonly ExpectValue True = new ExpectValue(true);

		public ExpectValue(object value) { this.Value = value; }

		public object Value { get; }

		public string ToString(IFormatter<object> formatter) => formatter.Format(Value);
		public override string ToString() => Value.ToString();
	}
}
