using System.Linq.Expressions;
using CheckThat.Formatting;

namespace CheckThat.Expectations
{
	class NotExpect : IExpect
    {
        readonly IExpect inner;

        public NotExpect(IExpect inner) { this.inner = inner; }

        public CheckResult Check() {
            var innerResult = inner.Check();
            return new CheckResult(!innerResult.IsSuccess, innerResult.Actual, innerResult.Expected);
        }

        public string FormatExpression(IFormatter<Expression> formatter) =>
			string.Format("!({0})", inner.FormatExpression(formatter));

		public ConeMessage FormatMessage(IFormatter<object> formatter) =>
			ConeMessage.Empty;
    }
}