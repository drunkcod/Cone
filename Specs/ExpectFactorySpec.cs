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
        public void treats_string_equality_specially() {
            var a = "a";
            var b = "b";

            Verify.That(() => ExpectFrom(() => a == b) is StringEqualExpect);
        }

        IExpect ExpectFrom(Expression<Func<bool>> expr) {
            return new ExpectFactory().From(expr.Body);
        }
    }
}
