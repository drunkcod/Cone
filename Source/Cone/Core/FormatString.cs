using System.Text.RegularExpressions;

namespace Cone.Core
{
    public class FormatString
    {
        static readonly Regex FormatStringPattern = new Regex(@"\{((\d)(,.+?)?(:.+?)?)\}", RegexOptions.Compiled);

        readonly string format;

        public FormatString(string format) {
            this.format = format;
        }

        public bool HasItemFormat { get { return FormatStringPattern.Match(format).Success; } }

        public string Format(params object[] args) { return string.Format(ToString(), args); }

        public override string ToString() { return format; }
    }
}
