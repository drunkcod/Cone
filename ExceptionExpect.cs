using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExceptionExpect : IExpect
    {
        public const string MissingExceptionFormat = "{0} didn't raise an exception.";
        public const string UnexpectedExceptionFormat = "{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}";

        readonly Expression expr;
        readonly object actual;
        readonly Type expected;

        public ExceptionExpect(Expression<Action> expr, Type expectedExceptionType) {
            this.expr = expr;
            this.actual = Invoke(expr);
            this.expected = expectedExceptionType;
        }

        public object Check(Action<string> onCheckFailed, ExpressionFormatter formatter) {
            if(actual == null)
                onCheckFailed(FormatMissing(formatter));
            else if(!expected.IsAssignableFrom(actual.GetType()))
                onCheckFailed(FormatUnexpected(formatter));
            return actual;
        }

        static object Invoke(Expression<Action> expr) {
            try {
                expr.Compile()();
            } catch(Exception e) {
                return e;
            }
            return null;
        }

        string FormatMissing(ExpressionFormatter formatter) {
            return string.Format(MissingExceptionFormat, formatter.Format(expr));
        }

        string FormatUnexpected(ExpressionFormatter formatter) {
            return string.Format(UnexpectedExceptionFormat,
                formatter.Format(expr), expected, actual.GetType());
        }
    }
}
