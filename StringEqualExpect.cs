using System;
using System.Linq.Expressions;

namespace Cone
{
    public class StringEqualExpect : BinaryExpect
    {
        const int DisplayWidth = 62;
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
            Console.WriteLine("{0} {1} {2}", input, position, width);
            Console.WriteLine(width - prefix.Length - postfix.Length);

            var start = first + prefix.Length;

            var value = input.Substring(start, Math.Min(width - prefix.Length - postfix.Length, input.Length - start));

            return string.Format("{0}{1}{2}", prefix, value, postfix);      
        }

        public override string FormatMessage(IFormatter<object> formatter) {
            var n = ActualString.IndexOfDifference(ExpectedString);
            var displayActual = formatter.Format(Center(ActualString, n, DisplayWidth));
            var displayExpected = formatter.Format(Center(ExpectedString, n, DisplayWidth));

            var format = string.Format("{0}\n{1}\n{2}^", Preamble, EqualFormat, 
                new string('-', displayActual.IndexOfDifference(displayExpected) + EqualFormat.IndexOf('{')));
            return string.Format(format, displayActual, displayExpected);
        }

        string ActualString { get { return (string)actual; } }
        string ExpectedString { get { return (string)expected; } }
    }
}
