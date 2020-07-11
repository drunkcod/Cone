using System.Linq.Expressions;
using Cone.Expectations;
using CheckThat;

namespace Cone
{
    [Describe(typeof(BooleanExpect))]
    public class BooleanExpectSpec
    {
        public void uses_equal_format_for_messages() {
            Check.That(() => new BooleanExpect(Expression.Constant(false), new ExpectValue(true)).MessageFormat("true", "false").ToString() == ExpectMessages.EqualFormat("true", "false").ToString());
        }
    }
}
