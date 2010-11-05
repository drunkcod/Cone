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
        int A = 1, B = 2;

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

        [DisplayAs("A == B")]
        public void EqualExpect() {
            Verify.That(() => ExpectFrom(() => A == B) is EqualExpect);
        }

        [DisplayAs("A != B")]
        public void NotEqualExpect() {
            Verify.That(() => ExpectFrom(() => A != B) is NotEqualExpect);
        }

        [DisplayAs("A < B")]
        public void LessThanExpect() {
            Verify.That(() => ExpectFrom(() => A < B) is LessThanExpect);
        }

        [DisplayAs("A <= B")]
        public void LessThanOrEqualExpect() {
            Verify.That(() => ExpectFrom(() => A <= B) is LessThanOrEqualExpect);
        }

        [DisplayAs("A > B")]
        public void GreaterThanExpect() {
            Verify.That(() => ExpectFrom(() => A > B) is GreaterThanExpect);
        }

        [DisplayAs("A >= B")]
        public void GreaterThanOrEqualExpect() {
            Verify.That(() => ExpectFrom(() => A >= B) is GreaterThanOrEqualExpect);
        }

        IExpect ExpectFrom(Expression<Func<bool>> expression) {
            return Expectory.From(expression.Body);
        }
    }
}
