using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Expectations
{
    public class StringContainsProvider : IMethodExpectProvider
    {
        IEnumerable<MethodInfo> IMethodExpectProvider.GetSupportedMethods() {
            return new[]{ typeof(string).GetMethod("Contains") };
        }

        IExpect IMethodExpectProvider.GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
            return new StringMethodExpect("string containing", body, method, target, args);
        }
    }

    public class StringMethodExpect : MethodExpect
    {
        readonly string methodDisplay;

        public StringMethodExpect(string methodDisplay, Expression body, MethodInfo method, object target, object[] arguments):
            base(body, method, target, arguments) {
            this.methodDisplay = methodDisplay;
        }

        protected override string FormatExpected(IFormatter<object> formatter) {
            return string.Format("{0} {1}", methodDisplay, formatter.Format(arguments[0]));
        }
    }
}
