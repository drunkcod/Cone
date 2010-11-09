using System;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    public class ExceptionExpect : Expect
    {
        public ExceptionExpect(Expression<Action> expression, Type expected)
            : base(expression.Body, Invoke(expression), expected) {
        }

        Type ExpectedExceptionType { get { return (Type)Expected; } }

        protected override bool CheckCore() {
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
                return string.Format(ExpectMessages.MissingExceptionFormat, formatter.Format(body));
            return string.Format(ExpectMessages.UnexpectedExceptionFormat,
                formatter.Format(body), Expected, actual.GetType());
        }
    }
}
