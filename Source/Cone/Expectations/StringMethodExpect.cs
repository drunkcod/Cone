using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
{
    public delegate string MethodDisplay(object[] parameters);
    
    public class StringMethodsProvider : IMethodExpectProvider
    {
        readonly Dictionary<MethodInfo, MethodDisplay> methodDisplay;

        public StringMethodsProvider() {
            var s = typeof(string);
            var t = new[]{ s };
            methodDisplay = new Dictionary<MethodInfo, MethodDisplay> {
                { s.GetMethod("Contains"), _ => "a string containing {0}" },
                { s.GetMethod("EndsWith", t), _ => "a string ending with {0}" },
                { s.GetMethod("EndsWith", new []{ s, typeof(StringComparison) }), x => string.Format("a string ending with {{0}} using '{0}'", x[1]) },
                { s.GetMethod("EndsWith", new []{ s, typeof(bool), typeof(CultureInfo) }), x => {
                    var culture = (CultureInfo)x[2];
                    var cultureName = culture == CultureInfo.InvariantCulture ? "InvariantCulture" : culture.Name;
                    return string.Format("a string ending with {{0}} using '{0}'{1}", cultureName, (bool)x[1] ? " & ignoring case" : ""); 
                } },
                { s.GetMethod("StartsWith", t), _ => "a string starting with {0}" }
            };
        }

        public IEnumerable<MethodInfo> GetSupportedMethods() {
            return methodDisplay.Keys;
        }

        public IExpect GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
            return new StringMethodExpect(methodDisplay[method], body, method, target, args);
        }
    }

    public class StringMethodExpect : MemberMethodExpect
    {
        readonly MethodDisplay methodDisplay;

        public StringMethodExpect(MethodDisplay methodDisplay, Expression body, MethodInfo method, object target, object[] arguments):
            base(body, method, target, arguments) {
            this.methodDisplay = methodDisplay;
        }

        public override string FormatExpected(IFormatter<object> formatter) {
            return string.Format(methodDisplay(arguments), formatter.Format(arguments[0]));
        }
    }
}
