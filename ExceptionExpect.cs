using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExceptionExpect : IExpect
    {
        public const string MissingExceptionFormat = "{0} didn't raise an exception.";
        public const string UnexpectedExceptionFormat = "{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}";

        readonly Expression<Action> expr;
        readonly Type expectedExceptionType;

        public ExceptionExpect(Expression<Action> expr, Type expectedExceptionType) {
            this.expr = expr;
            this.expectedExceptionType = expectedExceptionType;
        }

        public object Check(Action<string> onCheckFailed, ExpressionFormatter formatter) {
            try {
                expr.Compile()();
                onCheckFailed(FormatMissing(formatter));
                return null;
            } catch (Exception e) {
                if (expectedExceptionType.IsAssignableFrom(e.GetType()))
                    return e;
                onCheckFailed(FormatUnexpected(e, formatter));
                return null;
            }
        }

        string FormatMissing(ExpressionFormatter formatter) {
            return string.Format(MissingExceptionFormat, formatter.Format(expr));
        }

        string FormatUnexpected(Exception e, ExpressionFormatter formatter) {
            return string.Format(UnexpectedExceptionFormat,
                formatter.Format(expr), expectedExceptionType, e.GetType());
        }
    }
}
