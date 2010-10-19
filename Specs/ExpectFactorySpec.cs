using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Cone.Expectations;

namespace Cone
{
    [Describe(typeof(ExpectFactory))]
    public class ExpectFactorySpec
    {
        ExpectFactory Expectory = new ExpectFactory();

        public void special_case_string_equality() {
            string a = "a", b = "b";

            Verify.That(() => ExpectFrom(() => a == b) is StringEqualExpect);
        }

        [DisplayAs("special case \"is\" test")]
        public void special_case_type_check() {
            var obj = new object();

            Verify.That(() => ExpectFrom(() => obj is string) is EqualExpect);
            
            var expect = (EqualExpect)ExpectFrom(() => obj is string);
            Verify.That(() => expect.Actual == typeof(object));
            Verify.That(() => expect.Expected == typeof(string));
        }

        IExpect ExpectFrom(Expression<Func<bool>> expression) {
            return Expectory.From(expression.Body);
        }
    }
}
