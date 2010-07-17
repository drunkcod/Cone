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

    public class Expect
    {
        public const string EqualFormat = "  {0} wasn't equal to {1}";
        public const string EqualValuesFormat = "  Expected: {1}\n  But was: {0}";
        public const string NotEqualFormat = "  {0} was equal to {1}";
        public const string NotEqualValuesFormat = "  Didn't expect {1}";
        public const string FailFormat = "  {0} failed.";

        static readonly ConstructorInfo expector = typeof(Expect).GetConstructor(new[] { typeof(object), typeof(string) });
        static readonly ConstructorInfo binaryExpector = typeof(BinaryExpect).GetConstructor(new[] { typeof(object), typeof(object), typeof(string), typeof(string) });
        
        protected readonly ExpressionFormatter formatter = new ExpressionFormatter();
        protected readonly object actual;
        protected readonly string format;

        public static Expect Equal(object actual, string format) {
            return new Expect(actual, format); 
        }

        public static Expect Equal(object actual, object expected, string format, string formatValues) {
            return new BinaryExpect(actual, expected, format, formatValues);
        }

        public static Expression<Func<Expect>> Lambda(Expression body, Expression actual, string format, string formatValues) {
            BinaryExpression binary = body as BinaryExpression;
            if (binary != null)
                return Lambda(binary, format, formatValues);
            else
                return Expression.Lambda<Func<Expect>>(
                    Expression.New(expector,
                            Expression.TypeAs(actual, typeof(object)),
                            Expression.Constant(format)));
        }

        public static Expression<Func<Expect>> Lambda(BinaryExpression body, string format, string formatValues) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(binaryExpector,
                        Expression.TypeAs(body.Left, typeof(object)),
                        Expression.TypeAs(body.Right, typeof(object)),
                        Expression.Constant(format), Expression.Constant(formatValues)));
        }


        public Expect(object actual, string format) {
            this.actual = actual;
            this.format = format;
        }

        public bool Check() { 
            return Expected.Equals(actual);
        }

        public virtual string Format(Expression expr) {
            return string.Format(format, formatter.Format(expr));
        }

        public virtual string Format(params string[] args) {
            return string.Format(format, args);
        }

        protected virtual object Expected { get { return true; } }
    }

    public class BinaryExpect : Expect
    {
        static readonly ExpectNull ExpectNull = new ExpectNull();

        readonly object expected;
        readonly string formatValues;
        
        public BinaryExpect(object actual, object expected, string format, string formatValues) : base(actual, format) {
            this.expected = expected;
            this.formatValues = formatValues;
        }

        override public string Format(Expression expr) {
            return formatter.FormatBinary(expr, GetBinaryOp) + "\n" + FormatValues();
        }

        override public string Format(params string[] args) {
            return string.Format(format, args) + "\n" + FormatValues();
        }

        protected string FormatValues() {
            return string.Format(formatValues, actual, expected);
        }

        protected override object Expected {
            get {
                return expected ?? ExpectNull;
            }
        }

        static string GetBinaryOp(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal: return Expect.EqualFormat;
                case ExpressionType.NotEqual: return Expect.NotEqualFormat;
            }
            throw new NotSupportedException();
        }
    }
}
