using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(StringEqualExpect))]
    public class StringVerificationSpec
    {
        const Expression NoBody = null;

        public void preamble_when_lengths_differ() {
            StringEqualExpect Expect = new StringEqualExpect(NoBody, "Hello World", "Hello world!");

            Verify.That(() => Expect.Preamble == "Expected string length 12 but was 11.");
        }

        public void preamble_with_equal_lengths() {
            StringEqualExpect Expect = new StringEqualExpect(NoBody, "Hello World", "Hello World");

            Verify.That(() => Expect.Preamble == "String lengths are both 11.");
        }

        public void find_index_of_first_difference() {
            StringEqualExpect Expect = new StringEqualExpect(NoBody, "0123456", "012345?");

            Verify.That(() => Expect.IndexOfFirstDifference == 6);
        }

        [Row("0123456789", 0, 7, "0123...")
        ,Row("0123456789", 6, 7, "...6789") 
        ,Row("0123456789", 4, 7, "...4...")
        ,Row("0123", 3, 4, "0123")]
        public void center_message_on(string input, int position, int width, string output)
        {
            Verify.That(() => StringEqualExpect.Center(input, position, width) == output);
        }
    }
}
