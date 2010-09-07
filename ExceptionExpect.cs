using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExceptionExpect : ExpectBase
    {
        public const string MissingExceptionFormat = "{0} didn't raise an exception.";
        public const string UnexpectedExceptionFormat = "{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}";

        public ExceptionExpect(Expression<Action> body, Type expected) 
            : base(body, expected, Invoke(body))  {
        }

        Type ExpectedExceptionType { get { return (Type)expected; } }

        protected override void CheckCore(Action<string> onCheckFailed, ExpressionFormatter formatter) {
            if(actual == null)
                onCheckFailed(FormatMissing(formatter));
            else if(!ExpectedExceptionType.IsAssignableFrom(actual.GetType()))
                onCheckFailed(FormatUnexpected(formatter));
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
            return string.Format(MissingExceptionFormat, formatter.Format(body));
        }

        string FormatUnexpected(ExpressionFormatter formatter) {
            return string.Format(UnexpectedExceptionFormat,
                formatter.Format(body), expected, actual.GetType());
        }
    }
}
