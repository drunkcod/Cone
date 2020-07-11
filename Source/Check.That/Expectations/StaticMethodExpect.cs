using System.Linq.Expressions;
using System.Reflection;

namespace CheckThat.Expectations
{
    public class StaticMethodExpect : MethodExpect
    {
        public StaticMethodExpect(Expression body, MethodInfo method, object[] arguments) : base(body, method, arguments) { }

        protected override object Target => null;

        protected override object Actual => arguments[0];
    }
}
