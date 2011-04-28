using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Expectations
{
    public class StaticMethodExpect : MethodExpect
    {
        public StaticMethodExpect(Expression body, MethodInfo method, object[] arguments) : base(body, method, arguments) { }

        protected override object Target { get { return null; } }

        protected override object Actual { get { return arguments[0]; } }
    }
}
