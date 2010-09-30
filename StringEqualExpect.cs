using System;
using System.Linq.Expressions;

namespace Cone
{
    public class StringEqualExpect : BinaryExpect
    {
        public StringEqualExpect(Expression body, string actual, string expected) : base(body, actual, expected) { }

        public string Preamble { 
            get {
                var actualLength = ActualString.Length;
                var expectedLength = ExpectedString.Length;
                if(actualLength == expectedLength)
                    return string.Format("String lengths are both {0}.", actualLength);
                return string.Format("Expected string length {0} but was {1}.", expectedLength, actualLength); 
            } 
        }

        public int IndexOfFirstDifference {
            get {
                int i = 0, end = Math.Min(ActualString.Length, ExpectedString.Length);
                for(; i != end; ++i)
                    if(ActualString[i] != ExpectedString[i])
                        return i;
                return end;
            }
        }

        public static string Center(string input, int position, int width) {
            if(input.Length <= width)
                return input;
            var left = width / 2;

            var prefix = string.Empty;
            var postfix = string.Empty;

            var start = Math.Max(0, position - left);
            if(start > 0) {
                prefix = "...";
            }
            var end = position + 1 + left;

            if(end < input.Length)
                postfix = "...";
            var value = input.Substring(start + prefix.Length, width - prefix.Length - postfix.Length);

            return string.Format("{0}{1}{2}", prefix, value, postfix);      
        }

        public override string FormatMessage(IFormatter<object> formatter) {
            var format = string.Format("{0}\n{1}\n{2}^", Preamble, EqualFormat, 
                new string('-', 1 + IndexOfFirstDifference + EqualFormat.IndexOf('{')));
            return string.Format(format, formatter.Format(actual), formatter.Format(expected));
        }

        string ActualString { get { return (string)actual; } }
        string ExpectedString { get { return (string)expected; } }
    }
}
