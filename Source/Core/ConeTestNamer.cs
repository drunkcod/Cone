using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Cone.Core
{
    class FormatString
    {
        static readonly Regex FormatStringPattern = new Regex(@"\{((?<id>\d)(,.+?)?(:.+?)?)\}", RegexOptions.Compiled);
        static readonly int IdGroup = FormatStringPattern.GroupNumberFromName("id");

        readonly string format;
        readonly SortedDictionary<int, string> parts = new SortedDictionary<int,string>();

        public FormatString(string format) 
        {
            this.format = format;
            foreach(Match item in FormatStringPattern.Matches(format))
                parts.Add(int.Parse(item.Groups[IdGroup].Value), item.Value);
        }

        public bool HasItemFormat { get { return parts.Count != 0; } }

        public string Format(params object[] args) { return string.Format(ToString(), args); }

        public override string ToString() { return format; }
    }

    public class ConeTestNamer
    {
        static readonly Regex NormalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);

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
            var formatString = new FormatString(baseName);
            var displayParameters = DisplayParameters(method.GetParameters(), parameters);
            if(formatString.HasItemFormat)
                return formatString.Format(displayParameters);
            return string.Format("{0}({1})", baseName, FormatParameters(displayParameters));
        }

        object[] DisplayParameters(ParameterInfo[] info, object[] parameters) {
            var result = new object[parameters.Length];
            for(var i = 0; i != parameters.Length; ++i) {
                var displayClassAttribute = info[i].GetCustomAttributes(typeof(DisplayClassAttribute), true);
                if(displayClassAttribute.Length == 0)
                    result[i] = parameters[i];
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
                result.Append(sep).Append(Format(arguments[i]));
            return result;
        }
    }
}
