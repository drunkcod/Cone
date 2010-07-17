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

    public class Expect
    {
        public const string FormatExpression = "  {0} failed";

        static readonly ConstructorInfo expector = typeof(Expect).GetConstructor(new[] { typeof(Expression), typeof(object) });
        static readonly ConstructorInfo binaryExpector = typeof(BinaryExpect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object), typeof(BinaryExpectFormat) });
        
        protected readonly ExpressionFormatter formatter = new ExpressionFormatter();
        protected readonly Expression body;
        protected readonly object actual;

        public static Expression<Func<Expect>> Lambda(Expression body) {
            var binary = body as BinaryExpression;
            if (binary != null)
                return Lambda(binary);
            return Expression.Lambda<Func<Expect>>(
                Expression.New(expector,
                        Expression.Constant(body),
                        Expression.TypeAs(body, typeof(object))));
        }

        static Expression<Func<Expect>> Lambda(BinaryExpression body) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(binaryExpector,
                        Expression.Constant(body),
                        Expression.TypeAs(body.Left, typeof(object)),
                        Expression.TypeAs(body.Right, typeof(object)),
                        Expression.Constant(BinaryExpect.GetBinaryFormat(body.NodeType))));
        }

        public Expect(Expression body, object actual) {
            this.body = body;
            this.actual = actual;
        }

        public bool Check() { 
            return Expected.Equals(actual);
        }

        public virtual string Format() {
            return string.Format(FormatExpression, formatter.Format(body));
        }

        public virtual string Format(params string[] args) {
            return string.Format(FormatExpression, args);
        }

        protected virtual object Expected { get { return true; } }
    }

    public class BinaryExpect : Expect
    {
        public static readonly BinaryExpectFormat EqualFormat = new BinaryExpectFormat("  {0} wasn't equal to {1}", "  Expected: {1}\n  But was: {0}");
        public static readonly BinaryExpectFormat NotEqualFormat = new BinaryExpectFormat("  {0} was equal to {1}", "  Didn't expect {1}");

        static readonly ExpectNull ExpectNull = new ExpectNull();

        readonly object expected;
        readonly BinaryExpectFormat format;
        
        public BinaryExpect(Expression body, object actual, object expected, BinaryExpectFormat format) : base(body, actual) {
            this.expected = expected;
            this.format = format;
        }

        override public string Format() {
            return formatter.FormatBinary(body, GetBinaryOp) + "\n" + FormatValues();
        }

        override public string Format(params string[] args) {
            return string.Format(format.FormatExpression, args) + "\n" + FormatValues();
        }

        protected string FormatValues() {
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
