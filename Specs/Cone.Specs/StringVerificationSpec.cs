using System.Linq.Expressions;
using Cone.Expectations;

namespace Cone
{
    [Describe(typeof(StringEqualExpect))]
    public class StringVerificationSpec
    {
        readonly BinaryExpression IgnoredBody = Expression.MakeBinary(ExpressionType.Equal, Expression.Constant("a"), Expression.Constant("b"));

        public void preamble_when_lengths_differ() {
            StringEqualExpect Expect = new StringEqualExpect(IgnoredBody, "Hello World", "Hello world!");

            Check.That(() => Expect.Preamble == "Expected string length 12 but was 11.");
        }

        public void preamble_with_equal_lengths() {
            StringEqualExpect Expect = new StringEqualExpect(IgnoredBody, "Hello World", "Hello World");

            Check.That(() => Expect.Preamble == "String lengths are both 11.");
        }

        [Row("0123456789", 0, 7, "012345…")
        ,Row("0123456789", 6, 7, "…456789")
        ,Row("0123456789", 4, 7, "…23456…")
        ,Row("0123", 3, 4, "0123")]
        public void center_message_on(string input, int position, int width, string output)
        {
            Check.That(() => StringEqualExpect.Center(input, position, width) == output);
        }
    }
}
