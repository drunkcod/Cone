using System.Reflection;
using System.Text.RegularExpressions;
using System;

namespace Cone.Core
{
    public struct FormatString
    {
        static readonly Regex FormatStringPattern = new Regex(@"\{\d(?:,.+?)?(?::.+?)?\}|\{(?<named>.+?)\}", RegexOptions.Compiled);
		static readonly int NamedGroupIndex = FormatStringPattern.GroupNumberFromName("named");

        readonly string format;

        public FormatString(string format) {
            this.format = format;
        }

        public bool HasItemFormat => FormatStringPattern.Match(format).Success;

        public string Format(ParameterInfo[] parameters, object[] values, Func<object, string> formatNamed) =>
			FormatStringPattern.Replace(format, m => { 
				var named = m.Groups[NamedGroupIndex];
				if(named.Success) {
					var n = Array.FindIndex(parameters, x => x.Name == named.Value);
					return n >= 0
					? formatNamed(values[n])
					: m.Value;
				}
				return string.Format(m.Value, values);
			});

        public override string ToString() { return format; }
    }
}
