using System.Linq.Expressions;
namespace Cone.Expectations
{
    class NotExpect : IExpect
    {
        readonly IExpect inner;

        public NotExpect(IExpect inner) { this.inner = inner; }

        public bool Check(out object actual) {
            return !inner.Check(out actual);
        }

        public string FormatExpression(IFormatter<Expression> formatter) {
            return string.Format("!({0})", inner.FormatExpression(formatter));
        }

        public string FormatMessage(IFormatter<object> formatter) {
            return string.Empty;
        }
    }
}