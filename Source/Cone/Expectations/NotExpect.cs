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

        string IExpect.FormatActual(IFormatter<object> formatter) {
            return inner.FormatActual(formatter);
        }

        string IExpect.FormatExpected(IFormatter<object> formatter) {
            return inner.FormatExpected(formatter);
        }

        public string FormatMessage(IFormatter<object> formatter) {
            return string.Empty;
        }
    }
}