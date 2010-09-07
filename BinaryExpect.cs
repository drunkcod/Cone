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
        public static readonly BinaryExpectFormat EqualFormat = new BinaryExpectFormat("{0} == {1}", "  Expected: {1}\n  But was: {0}");
        public static readonly BinaryExpectFormat NotEqualFormat = new BinaryExpectFormat("{0} != {1}", "  Didn't expect both to be {1}");

        static readonly ExpectNull ExpectNull = new ExpectNull();
        static readonly ConstructorInfo BinaryExpectCtor = typeof(BinaryExpect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });

        public static Expect From(BinaryExpression body) {
            return Expect.From(BinaryExpectCtor, body, body.Left, body.Right);
        }

        public BinaryExpect(Expression body, object actual, object expected)
            : base(body, actual, expected ?? ExpectNull) {
        }

        public override string FormatBody(ExpressionFormatter formatter) {
            return formatter.FormatBinary(body, GetBinaryOp);
        }

        public override string FormatValues(ExpressionFormatter formatter) {
            var format = GetBinaryFormat(body.NodeType);
            return string.Format(format.FormatValues, actual, expected);
        }

        internal static BinaryExpectFormat GetBinaryFormat(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal: return BinaryExpect.EqualFormat;
                case ExpressionType.NotEqual: return BinaryExpect.NotEqualFormat;
            }
            throw new NotSupportedException();
        }

        public override bool Check() {
            return base.Check() ^ body.NodeType == ExpressionType.NotEqual;
        }

        static string GetBinaryOp(ExpressionType nodeType) {
            return GetBinaryFormat(nodeType).FormatExpression;
        }
    }
}
