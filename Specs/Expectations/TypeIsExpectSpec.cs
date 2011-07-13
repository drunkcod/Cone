using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    [Describe(typeof(TypeIsExpect))]
    public class TypeIsExpectSpec
    {

        public void null_handling() {
            object obj = null;
            var typeIs = new TypeIsExpect(Body(() => obj is string), null, typeof(string));
            Verify.That(() => typeIs.Check().Success == false);
        }

        public void result_is_actual_object() {
            var obj = "Hello World";
            var typeIs = new TypeIsExpect(Body(() => obj is string), obj, typeof(string));
            Verify.That(() => Object.ReferenceEquals(typeIs.Check().Actual, obj));
        }

        Expression Body(Expression<Func<bool>> body) { return body; }
    }
}
