using System;
using System.Linq.Expressions;
using System.Reflection;

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

    public struct BinaryExpectFormat
    {
        public readonly string FormatExpression;
        public readonly string FormatValues;

        public BinaryExpectFormat(string formatExpression, string formatValues) {
            this.FormatExpression = formatExpression;
            this.FormatValues = formatValues;
        }
    }

    public class BinaryExpect : Expect
    {
        public static readonly BinaryExpectFormat EqualFormat = new BinaryExpectFormat("  {0} wasn't equal to {1}", "  Expected: {1}\n  But was: {0}");
        public static readonly BinaryExpectFormat NotEqualFormat = new BinaryExpectFormat("  {0} was equal to {1}", "  Didn't expect {1}");

        static readonly ExpectNull ExpectNull = new ExpectNull();
        static readonly ConstructorInfo binaryExpector = typeof(BinaryExpect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object), typeof(BinaryExpectFormat) });

        public static Expression<Func<Expect>> Lambda(BinaryExpression body) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(binaryExpector,
                    Expression.Constant(body),
                    Expression.TypeAs(body.Left, typeof(object)),
                    Expression.TypeAs(body.Right, typeof(object)),
                    Expression.Constant(BinaryExpect.GetBinaryFormat(body.NodeType))));
        }

        readonly object expected;
        readonly BinaryExpectFormat format;

        public BinaryExpect(Expression body, object actual, object expected, BinaryExpectFormat format)
            : base(body, actual) {
            this.expected = expected;
            this.format = format;
        }

        override public string Format(ExpressionFormatter formatter) {
            return formatter.FormatBinary(body, GetBinaryOp) + "\n" + FormatValues();
        }

        override public string Format(params string[] args) {
            return string.Format(format.FormatExpression, args) + "\n" + FormatValues();
        }

        string FormatValues() {
            return string.Format(format.FormatValues, actual, expected);
        }

        protected override object Expected {
            get { return expected ?? ExpectNull; }
        }

        internal static BinaryExpectFormat GetBinaryFormat(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal: return BinaryExpect.EqualFormat;
                case ExpressionType.NotEqual: return BinaryExpect.NotEqualFormat;
            }
            throw new NotSupportedException();
        }

        static string GetBinaryOp(ExpressionType nodeType) {
            return GetBinaryFormat(nodeType).FormatExpression;
        }
    }
}
