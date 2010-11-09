using System;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    public class StringEqualExpect : EqualExpect
    {
        const int DisplayWidth = 62;

        public StringEqualExpect(Expression body, string actual, string expected) : base(body, actual, expected) { }

        public string Preamble { 
            get {
                if(ActualValue.Length == ExpectedValue.Length)
                    return string.Format("String lengths are both {0}.", ActualValue.Length);
                return string.Format("Expected string length {0} but was {1}.", ExpectedValue.Length, ActualValue.Length); 
            } 
        }

        public static string Center(string input, int position, int width) {
            if(input.Length <= width)
                return input;
            var left = width / 2;

            var prefix = string.Empty;
            var postfix = string.Empty;

            var first = Math.Max(0, position - left);
            if(first > 0) {
                prefix = "...";
            }
            var end = position + 1 + left;

            if(end < input.Length)
                postfix = "...";

            var start = first + prefix.Length;

            var value = input.Substring(start, Math.Min(width - prefix.Length - postfix.Length, input.Length - start));

            return string.Format("{0}{1}{2}", prefix, value, postfix);      
        }

        public override string FormatMessage(IFormatter<object> formatter) {
            var n = ActualValue.IndexOfDifference(ExpectedValue);
            var displayActual = formatter.Format(Center(ActualValue, n, DisplayWidth));
            var displayExpected = formatter.Format(Center(ExpectedValue, n, DisplayWidth));

            var format = string.Format("{0}\n{1}\n{2}^", Preamble, MessageFormat, 
                new string('-', displayActual.IndexOfDifference(displayExpected) + ExpectMessages.EqualFormat.IndexOf('{')));
            return string.Format(format, displayActual, displayExpected);
        }

        string ActualValue { get { return Actual.ToString(); } }
        string ExpectedValue { get { return Expected.ToString(); } }
    }
}
