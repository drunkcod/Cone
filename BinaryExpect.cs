using System;
using System.Linq.Expressions;

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

    public class BinaryExpect : Expect
    {
        public const string EqualFormat = "  Expected: {1}\n  But was:  {0}";
        public const string NotEqualFormat = "  Didn't expect both to be {1}";

        static readonly ExpectNull ExpectNull = new ExpectNull();

        ExpressionType nodeType;

        public BinaryExpect(Expression body, object actual, object expected): this(body, body.NodeType, actual, expected) { }

        public BinaryExpect(Expression body, ExpressionType nodeType, object actual, object expected)
            : base(body, actual, expected ?? ExpectNull) {
            this.nodeType = nodeType;
        }
        public override string FormatMessage(IFormatter<object> formatter) {
            var format = GetBinaryFormat(body.NodeType);
            return string.Format(format, formatter.Format(actual), formatter.Format(expected));
        }

        internal static string GetBinaryFormat(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal: return BinaryExpect.EqualFormat;
                case ExpressionType.NotEqual: return BinaryExpect.NotEqualFormat;
            }
            return string.Empty;
        }

        public override bool Check() {
            if (nodeType == ExpressionType.Equal)
                return expected.Equals(actual);
            return Expression.Lambda<Func<bool>>(
                Expression.MakeBinary(nodeType, Expression.Constant(actual), Expression.Constant(expected)))
                .Execute();
        }
    }
}
