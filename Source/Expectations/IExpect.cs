using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Expectations
{
	public interface IExpect
    {
        CheckResult Check();
        string FormatExpression(IFormatter<Expression> formatter);
        string FormatActual(IFormatter<object> formatter);
        string FormatExpected(IFormatter<object> formatter);
        string FormatMessage(IFormatter<object> formatter);
    }
}