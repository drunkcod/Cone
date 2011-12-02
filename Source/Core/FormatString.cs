using System.Collections.Generic;
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
}
