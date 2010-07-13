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
    }

    class Expect
    {
        public const string EqualFormat = "  {0} wasn't equal to {1}.\n  Expected {3}\nActual {2}";
        public const string NotEqualFormat = "  {0} was equal to {1}.\n  Didn't expect {3}";
        public const string FailFormat = "  {0} failed.";
        
        static readonly ExpectNull ExpectNull = new ExpectNull();

        readonly object actual;
        readonly object expected;
        readonly string format;

        public static Expect Equal<TActual, TExpected>(TActual actual, TExpected expected, string format) { return new Expect(actual, expected, format); }

        public Expect(object actual, object expected, string format) {
            this.actual = actual;
            this.expected = expected ?? ExpectNull;
            this.format = format;
        }

        public bool Check() { 
            return expected.Equals(actual);
        }

        public string Format(Expression actualDisplay, Expression expectedDisplay) {
            return Format(Format(actualDisplay), Format(expectedDisplay));
        }

        public string Format(string actualDisplay, string expectedDisplay) {
            return string.Format(format, actualDisplay, expectedDisplay, actual, expected);
        }

        static string Format(Expression expr) {
            switch (expr.NodeType) {
                case ExpressionType.ArrayLength:
                    var unary = (UnaryExpression)expr;
                    return Format(unary.Operand) + ".Length";
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expr;
                    if (member.Expression.NodeType == ExpressionType.Constant)
                        return member.Member.Name;
                    return Format(member.Expression) + "." + member.Member.Name;
                case ExpressionType.Call:
                    var call = (MethodCallExpression)expr;
                    var args = new string[call.Arguments.Count];
                    for (int i = 0; i != args.Length; ++i)
                        args[i] = Format(call.Arguments[i]);
                    return FormatCallTarget(call) + "." + call.Method.Name + "(" + string.Join(", ", args) + ")";
            }
            return expr.ToString();
        }

        static string FormatCallTarget(MethodCallExpression call) {
            if (call.Object == null)
                return call.Method.DeclaringType.Name;
            return Format(call.Object);
        }
    }
}
