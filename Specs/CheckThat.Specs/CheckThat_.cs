using Xunit;

namespace CheckThat.Specs
{

	public class CheckThat_
	{
		[Fact]
		public void call_with_ref_struct_parameter() =>
			Check.That(() => StringFromSpan(new char[] { 'H', 'e', 'l', 'l', 'o' }) == "Hello");

		[Fact]
		public void call_with_ref_struct_parameter2() => Check
			.With(() => new char[] { 'H', 'e', 'l', 'l', 'o' })
			.That(x => StringFromSpan(x) == "Hello");

		static string StringFromSpan(Span<char> cs) => new(cs);
	}
}