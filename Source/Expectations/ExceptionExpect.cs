using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Expectations
{
    public class ExceptionExpect : Expect
    {
        public static ExceptionExpect From(Expression<Action> expression, Type expected) {
            return new ExceptionExpect(expression.Body, ExceptionOrNull(expression, expected), expected);
        }
       
        public static ExceptionExpect From<T>(Expression<Func<T>> expression, Type expected) {
            return new ExceptionExpect(expression.Body, ExceptionOrNull(expression.Body, expected), expected);
        }
       
        ExceptionExpect(Expression body, object result, Type expected): base(body, result, expected) { }

        Type ExpectedExceptionType { get { return (Type)Expected; } }

        protected override bool CheckCore() {
            return actual != null && ExpectedExceptionType.IsAssignableFrom(actual.GetType());
        }

        static object ExceptionOrNull(Expression expression, Type expected) {
            var eval = new ExpressionEvaluator();
            if(expected != typeof(NullSubexpressionException))
                eval.NullSubexpression = (e, c) => { throw new NullSubexpressionException(e, c); };
            var result = eval.Evaluate(expression, expression, x => x);
            if(result.IsError)
                return result.Exception;
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
