using System.Linq;
using System.Linq.Expressions;
using Cone.Expectations;
using CheckThat;

namespace Cone
{
	[Describe(typeof(StringEqualExpect))]
	public class StringVerificationSpec
	{
		readonly BinaryExpression IgnoredBody = Expression.MakeBinary(ExpressionType.Equal, Expression.Constant("a"), Expression.Constant("b"));

		public void preamble_when_lengths_differ() {
			var expect = new StringEqualExpect(IgnoredBody, "Hello World", "Hello world!");
			Check.That(() => expect.Preamble.ToString() == "Expected string length 12 but was 11.");
		}

		public void preamble_with_equal_lengths() {
			var expect = new StringEqualExpect(IgnoredBody, "Hello World", "Hello World");
			Check.That(() => expect.Preamble.ToString() == "String lengths are both 11.");
		}

		public void guide_aligns_to_difference() {
			var expect = new StringEqualExpect(IgnoredBody, "a", "b");
			var message = expect.FormatMessage(new ToStringFormatter());
			var lines = message.ToString().Split('\n');

			var lastLine = lines.Length - 1;
			var n = lines[lastLine].Length - 1;
			Check.That(
				() => lines.Length == 4,
				() => lines[lastLine - 2][n] == 'b', //expected
				() => lines[lastLine - 1][n] == 'a');//but was
		}

		public void guide_is_last_line() { 
			var expect = new StringEqualExpect(IgnoredBody, "a", "b");
			var linesWithGuide = expect.FormatMessage(new ToStringFormatter()).ToString().Split('\n');
			expect.IncludeGuide = false;
			var linesWithoutGuide = expect.FormatMessage(new ToStringFormatter()).ToString().Split('\n');

			Check.That(
				() => linesWithGuide.Length == 1 + linesWithoutGuide.Length,
				() => linesWithoutGuide.SequenceEqual(linesWithGuide.Take(linesWithoutGuide.Length)));
		}

		[Row("0123456789", 0, 7, "012345…")
		,Row("0123456789", 6, 7, "…456789")
		,Row("0123456789", 4, 7, "…23456…")
		,Row("0123", 3, 4, "0123")]
		public void center_message_on(string input, int position, int width, string output)
		{
			Check.That(() => StringEqualExpect.Center(input, position, width).ToString() == output);
		}
	}
}
