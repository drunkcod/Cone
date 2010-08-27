using System.Text.RegularExpressions;
using System.Reflection;

namespace Cone
{
    public class ConeTestNamer
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        readonly ParameterFormatter formatter = new ParameterFormatter();

        public string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        public string NameFor(MethodInfo method, object[] parameters) {
            if (parameters == null)
                return NameFor(method);
            var baseName = NameFor(method);
            var displayArguments = new string[parameters.Length];
            for (int i = 0; i != parameters.Length; ++i)
                displayArguments[i] = Format(parameters[i]);
            return baseName + "(" + string.Join(", ", displayArguments) + ")";
        }

        string Format(object obj) { return formatter.Format(obj); }
    }
}
