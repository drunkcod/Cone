using System;
using System.Linq.Expressions;
using CheckThat;
using CheckThat.Expectations;

namespace Cone.Expectations
{
	[Describe(typeof(TypeIsExpect))]
    public class TypeIsExpectSpec
    {
        public void null_handling() {
            object obj = null;
            var typeIs = new TypeIsExpect(Body(() => obj is string), typeof(object), new ExpectValue(null), typeof(string));
            Check.That(() => typeIs.Check().IsSuccess == false);
        }

        public void result_is_actual_object() {
            var obj = "Hello World";
            var typeIs = new TypeIsExpect(Body(() => obj is string), typeof(string), new ExpectValue(obj), typeof(string));
            Check.That(() => Object.ReferenceEquals(typeIs.Check().Actual.Value, obj));
        }

        Expression Body(Expression<Func<bool>> body) { return body; }
    }
}
