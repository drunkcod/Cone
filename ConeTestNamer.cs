using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System;
using System.Text;

namespace Cone
{
    public class ConeTestNamer
    {
        static readonly Regex NormalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        static readonly Regex IsFormatStringPattern = new Regex(@"\{(\d(,.+?)?(:.+?)?)\}", RegexOptions.Compiled);

        readonly ParameterFormatter formatter = new ParameterFormatter();

        public string NameFor(MethodBase method) {
             return string.Format(GetBaseName(method, x => x.Heading), DisplayParameters(method.GetParameters()));
        }

        public string GetBaseName(MethodBase method, Func<DisplayAsAttribute, string> selectName) {
            var nameAttribute = method.GetCustomAttributes(typeof(DisplayAsAttribute), true);
            if(nameAttribute.Length != 0)
                return selectName(((DisplayAsAttribute)nameAttribute[0]));
            return NormalizeNamePattern.Replace(method.Name, " ");
        }

        public string NameFor(MethodInfo method, object[] parameters) {
            return NameFor(method, parameters, GetBaseName(method, x => x.Name));
        }

        public string NameFor(MethodInfo method, object[] parameters, string baseName) {
            if (parameters == null)
                return baseName;
            var displayParameters = DisplayParameters(method.GetParameters(), parameters);
            if(IsFormatString(baseName))
                return string.Format(baseName, displayParameters);
            return string.Format("{0}({1})", baseName, FormatParameters(displayParameters));
        }

        object[] DisplayParameters(ParameterInfo[] info, object[] parameters) {
            var result = new object[parameters.Length];
            for(var i = 0; i != parameters.Length; ++i) {
                var displayClassAttribute = info[i].GetCustomAttributes(typeof(DisplayClassAttribute), true);
                if(displayClassAttribute.Length == 0)
                    result[i] = Format(parameters[i]);
                else
                    result[i] = (displayClassAttribute[0] as DisplayClassAttribute).DisplayFor(parameters[i]);
            }
            return result;
        }
        
        string[] DisplayParameters(ParameterInfo[] parameters) { return Array.ConvertAll(parameters, x => x.Name); }

        object Format(object obj) { return formatter.AsWritable(obj); }

        object FormatParameters(object[] arguments) {
            var result = new StringBuilder();
            var sep = "";
            for(var i = 0; i != arguments.Length; ++i, sep = ", ")
                result.Append(sep).Append(arguments[i]);
            return result;
        }

        bool IsFormatString(string s) {
            return IsFormatStringPattern.IsMatch(s);
        }
    }
}
