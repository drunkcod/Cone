using System.Linq.Expressions;
using CheckThat.Formatting;

namespace CheckThat.Expectations
{
	public interface IExpect
    {
        CheckResult Check();
        string FormatExpression(IFormatter<Expression> formatter);
        ConeMessage FormatMessage(IFormatter<object> formatter);
    }
}
