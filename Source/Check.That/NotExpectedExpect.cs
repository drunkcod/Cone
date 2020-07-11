using System.Linq.Expressions;
using Cone.Core;
using Cone.Expectations;

namespace CheckThat
{
	class NotExpectedExpect : IExpect
	{
		readonly Expression expression;
		readonly ConeMessage message;

		public NotExpectedExpect(Expression expression, ConeMessage message) {
			this.expression = expression;
			this.message = message;
		}

		public CheckResult Check() => new CheckResult(false, Maybe<object>.None, Maybe<object>.None);

		public string FormatExpression(IFormatter<Expression> formatter) => formatter.Format(expression);
		public ConeMessage FormatMessage(IFormatter<object> formatter) => message;
	}
}
