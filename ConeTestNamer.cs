using System.Text.RegularExpressions;
using System.Reflection;

namespace Cone
{
    public class ConeTestNamer
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        readonly IFormatter<object> formatter = new ParameterFormatter();

        public string NameFor(MethodBase method) {
            var nameAttribute = method.GetCustomAttributes(typeof(TestNameAttribute), true);
            if(nameAttribute.Length != 0)
                return ((TestNameAttribute)nameAttribute[0]).Name;
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        public string NameFor(MethodInfo method, object[] arguments) {
            if (arguments == null)
                return NameFor(method);
            return string.Format("{0}({1})", NameFor(method), FormatArguments(arguments));
        }

        string Format(object obj) { return formatter.Format(obj); }

        string FormatArguments(object[] arguments) {
            var displayArguments = new string[arguments.Length];
            for (int i = 0; i != arguments.Length; ++i)
                displayArguments[i] = Format(arguments[i]);
            return string.Join(", ", displayArguments);
        }
    }
}
