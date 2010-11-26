using System.Linq.Expressions;
using Cone.Expectations;

namespace Cone
{
    [Describe(typeof(BooleanExpect))]
    public class BooleanExpectSpec
    {
        public void uses_equal_format_for_messages() {
            Verify.That(() => new BooleanExpect(Expression.Constant(false), true).MessageFormat == ExpectMessages.EqualFormat);
        }
    }
}
