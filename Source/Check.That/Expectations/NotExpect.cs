using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Expectations
{
    class NotExpect : IExpect
    {
        readonly IExpect inner;

        public NotExpect(IExpect inner) { this.inner = inner; }

        public CheckResult Check() {
            var innerResult = inner.Check();
            return new CheckResult(!innerResult.IsSuccess, innerResult.Actual, innerResult.Expected);
        }

        public string FormatExpression(IFormatter<Expression> formatter) {
            return string.Format("!({0})", inner.FormatExpression(formatter));
        }

		public ConeMessage FormatMessage(IFormatter<object> formatter) =>
			ConeMessage.Empty;
    }
}