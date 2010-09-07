using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExceptionExpect
    {
        public const string MissingExceptionFormat = "{0} didn't raise an exception.";
        public const string UnexpectedExceptionFormat = "{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}";

        readonly Expression<Action> expr;

        public ExceptionExpect(Expression<Action> expr) {
            this.expr = expr;
        }

        public TException Check<TException>(Action<string> onCheckFailed, ExpressionFormatter formatter) where TException : Exception {
            try {
                expr.Compile()();
                onCheckFailed(FormatMissing(formatter));
                return null;
            } catch (TException expected) {
                return expected;
            } catch (Exception e) {
                onCheckFailed(FormatUnexpected(e, typeof(TException), formatter));
                return null;
            }

        }

        string FormatMissing(ExpressionFormatter formatter) {
            return string.Format(MissingExceptionFormat, formatter.Format(expr));
        }

        string FormatUnexpected(Exception e, Type expectedType, ExpressionFormatter formatter) {
            return string.Format("{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}",
                formatter.Format(expr), expectedType, e.GetType());
        }
    }
}
