using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Expectations
{
    public class StringMethodsProvider : IMethodExpectProvider
    {
        readonly Dictionary<MethodInfo, string> methodDisplay;

        public StringMethodsProvider() {
            var s = typeof(string);
            methodDisplay = new Dictionary<MethodInfo,string>()
            {
                { s.GetMethod("Contains"), "string containing" },
                { s.GetMethod("EndsWith", new[]{ s }), "a string ending with" }
            };
        }

        IEnumerable<MethodInfo> IMethodExpectProvider.GetSupportedMethods() {
            return methodDisplay.Keys;
        }

        IExpect IMethodExpectProvider.GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
            return new StringMethodExpect(methodDisplay[method], body, method, target, args);
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
