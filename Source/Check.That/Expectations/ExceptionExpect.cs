using System;
using System.Linq.Expressions;
using CheckThat.Expressions;
using CheckThat.Formatting;

namespace CheckThat.Expectations
{
	public class ExceptionExpect : Expect
    {
        public static ExceptionExpect From(Expression<Action> expression, Type expected) =>
			new ExceptionExpect(expression.Body, ExceptionOrNull(expression, expected), expected);
       
        public static ExceptionExpect From<T>(Expression<Func<T>> expression, Type expected) =>
			new ExceptionExpect(expression.Body, ExceptionOrNull(expression.Body, expected), expected);

        ExceptionExpect(Expression body, object result, Type expected): base(body, new ExpectValue(result), new ExpectValue(expected)) { }

        Type ExpectedExceptionType => (Type)ExpectedValue;

        protected override bool CheckCore() =>
			ActualValue != null && ExpectedExceptionType.IsAssignableFrom(ActualValue.GetType());

        static object ExceptionOrNull(Expression expression, Type expected) {
            var eval = new ExpressionEvaluator();
            if(expected != typeof(NullSubexpressionException))
                eval.NullSubexpression = (e, c) => { throw new NullSubexpressionException(e, c); };
            var result = eval.Evaluate(expression, expression, ExpressionEvaluatorParameters.Empty, x => x);
            if(result.IsError)
                return result.Exception;
            return null;
        }

        public override string FormatExpression(IFormatter<Expression> formatter) {
            if(ActualValue== null)
                return string.Format(ExpectMessages.MissingExceptionFormat, formatter.Format(body));
            return string.Format(ExpectMessages.UnexpectedExceptionFormat,
                formatter.Format(body), ExpectedValue, ActualValue.GetType());
        }
    }
}
