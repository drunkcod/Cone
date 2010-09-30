using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExceptionExpect : ExpectBase
    {
        public const string MissingExceptionFormat = "{0} didn't raise an exception.";
        public const string UnexpectedExceptionFormat = "{0} raised the wrong type of Exception\nExpected: {1}\nActual: {2}";

        public ExceptionExpect(Expression<Action> expression, Type expected)
            : base(expression.Body, Invoke(expression), expected) {
        }

        Type ExpectedExceptionType { get { return (Type)expected; } }

        public override bool Check() {
            return actual != null && ExpectedExceptionType.IsAssignableFrom(actual.GetType());
        }

        static object Invoke(Expression<Action> expr) {
            try {
                expr.Execute();
            } catch(Exception e) {
                return e;
            }
            return null;
        }

        public override string FormatExpression(IFormatter<Expression> formatter) {
            if(actual == null)
                return string.Format(MissingExceptionFormat, formatter.Format(body));
            return string.Format(UnexpectedExceptionFormat,
                formatter.Format(body), expected, actual.GetType());
        }
    }
}
