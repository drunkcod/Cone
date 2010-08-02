using System.Text.RegularExpressions;
using System.Reflection;

namespace Cone
{
    public static class ConeTestNamer
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);

        public static string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        public static string NameFor(MethodInfo method, object[] parameters) {
            if (parameters == null)
                return NameFor(method);
            var baseName = NameFor(method);
            var displayArguments = new string[parameters.Length];
            for (int i = 0; i != parameters.Length; ++i)
                displayArguments[i] = parameters[i].ToString();
            return baseName + "(" + string.Join(", ", displayArguments) + ")";
        }
    }
}
