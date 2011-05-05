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
            var t = new[]{ s };
            methodDisplay = new Dictionary<MethodInfo,string> {
                { s.GetMethod("Contains"), "string containing" },
                { s.GetMethod("EndsWith", t), "a string ending with" },
                { s.GetMethod("StartsWith", t), "a string starting with" }
            };
        }

        IEnumerable<MethodInfo> IMethodExpectProvider.GetSupportedMethods() {
            return methodDisplay.Keys;
        }

        IExpect IMethodExpectProvider.GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
            return new StringMethodExpect(methodDisplay[method], body, method, target, args);
        }
    }

    public class StringMethodExpect : MemberMethodExpect
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
