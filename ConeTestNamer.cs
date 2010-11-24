using System.Text.RegularExpressions;
using System.Reflection;

namespace Cone
{
    public class ConeTestNamer
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        readonly IFormatter<object> formatter = new ParameterFormatter();

        public string NameFor(MethodBase method) {
            var baseName = GetNameOf(method);
            var parameters = method.GetParameters();
            var displayParameters = new string[parameters.Length];
            for(int i = 0; i != parameters.Length; ++i)
                displayParameters[i] = parameters[i].Name;
            return string.Format(baseName, displayParameters);
        }

        public string GetNameOf(MethodBase method) {
            var nameAttribute = method.GetCustomAttributes(typeof(DisplayAsAttribute), true);
            if(nameAttribute.Length != 0)
                return ((DisplayAsAttribute)nameAttribute[0]).Name;
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        public string NameFor(MethodInfo method, object[] arguments) {
            var baseName = GetNameOf(method);
            if (arguments == null)
                return baseName;
            if(IsFormatString(baseName))
                return string.Format(baseName, DisplayArguments(arguments));
            return string.Format("{0}({1})", baseName, FormatArguments(arguments));
        }

        string[] DisplayArguments(object[] arguments) {
            var displayArguments = new string[arguments.Length];
            for (int i = 0; i != arguments.Length; ++i)
                displayArguments[i] = Format(arguments[i]);
            return displayArguments;
        }

        string Format(object obj) { return formatter.Format(obj); }

        string FormatArguments(object[] arguments) {
            return string.Join(", ", DisplayArguments(arguments));
        }

        bool IsFormatString(string s) {
            return Regex.IsMatch(s, @"\{\d\}");
        }
    }
}
