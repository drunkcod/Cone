using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone.Expectations;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(StringContainsExpect))]
    public class StringContainsSpec
    {
        public void check_success() {
            Expression<Func<bool>> body = () => "Hello".Contains("ll");
            IExpect expect = new StringContainsExpect(body, "Hello", "ll");
            object value = string.Empty;
            Verify.That(() => expect.Check(out value) == true);
            Verify.That(() => value.ToString() == "Hello");
        }

        public void check_fail() {
            Expression<Func<bool>> body = () => "123".Contains("ABC");
            IExpect expect = new StringContainsExpect(body, "123", "ABC");
            object value = string.Empty;
            Verify.That(() => expect.Check(out value) == false);
            Verify.That(() => value.ToString() == "123");
        }

        public void message_formatting() {
            Expression<Func<bool>> body = () => "123".Contains("ABC");
            IExpect expect = new StringContainsExpect(body, "123", "ABC");
            object value = string.Empty;
            var formatter = new ParameterFormatter();
            var expected = string.Format(ExpectMessages.EqualFormat, "\"123\"", "string containing \"ABC\"");
            Verify.That(() => expect.FormatMessage(formatter) == expected);
        }
    }
}
