using System;
using System.Text.RegularExpressions;

namespace Cone.Core
{
	public delegate bool TryGetFormatted(string name, out string result);

	public struct FormatString
    {
        static readonly Regex FormatStringPattern = new Regex(@"\{\d(?:,.+?)?(?::.+?)?\}|(\{{1,2})(?<named>.+?)\}", RegexOptions.Compiled);
		static readonly int NamedGroupIndex = FormatStringPattern.GroupNumberFromName("named");

        readonly string format;

        public FormatString(string format) {
            this.format = format;
        }

        public bool HasItemFormat => FormatStringPattern.Match(format).Success;

        public string Format(object[] values, TryGetFormatted formatNamed) =>
			FormatStringPattern.Replace(format, m => { 
				var named = m.Groups[NamedGroupIndex];
				if(named.Success) {
					string result;
					if(m.Groups[NamedGroupIndex - 1].Length == 1 && formatNamed(named.Value, out result))
						return result;
					else return m.Value;
				}
				return string.Format(m.Value, values);
			});

        public override string ToString() { return format; }
    }
}
