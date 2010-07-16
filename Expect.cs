using System.Linq.Expressions;
using System;
namespace Cone
{
    class ExpectNull
    {
        public override bool Equals(object obj) {
            return obj == null;
        }

        public override int GetHashCode() {
            return 0;
        }

        public override string ToString() {
            return "null";
        }
    }

    public class Expect
    {
        public const string EqualFormat = "  {0} wasn't equal to {1}";
        public const string EqualValuesFormat = "  Expected: {1}\n  But was: {0}";
        public const string NotEqualFormat = "  {0} was equal to {1}";
        public const string NotEqualValuesFormat = "  Didn't expect {1}";
        public const string FailFormat = "  {0} failed.";
        
        static readonly ExpectNull ExpectNull = new ExpectNull();

        readonly ExpressionFormatter formatter = new ExpressionFormatter();
        readonly object actual;
        readonly object expected;
        readonly string format;
        readonly string formatValues;

        public static Expect Equal<TActual, TExpected>(TActual actual, TExpected expected, string format, string formatValues) { return new Expect(actual, expected, format, formatValues); }

        public Expect(object actual, object expected, string format, string formatValues) {
            this.actual = actual;
            this.expected = expected ?? ExpectNull;
            this.format = format;
            this.formatValues = formatValues;
        }

        public bool Check() { 
            return expected.Equals(actual);
        }

        public string Format(Expression expr) {
            if (expr is BinaryExpression)
                return formatter.FormatBinary(expr, GetBinaryOp) + "\n" + FormatValues();
            return formatter.Format(expr) + " failed.\n";
        }

        public string Format(string actualDisplay, string expectedDisplay) {
            return string.Format(format, actualDisplay, expectedDisplay) + "\n" + FormatValues();
        }

        string FormatValues(){
            return string.Format(formatValues, actual, expected);
        }

        static string GetBinaryOp(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal: return " wasn't equal to ";
                case ExpressionType.NotEqual: return " was equal to ";
            }
            throw new NotSupportedException();
        }
    }
}
