using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(ExpectFactory))]
    public class ExpectFactorySpec
    {
        ExpectFactory Expectory = new ExpectFactory();

        public void treats_string_equality_specially() {
            string a = "a", b = "b";

            Verify.That(() => ExpectFrom(() => a == b) is StringEqualExpect);
        }

        IExpect ExpectFrom(Expression<Func<bool>> expression) {
            return Expectory.From(expression.Body);
        }
    }
}
