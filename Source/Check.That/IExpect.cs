using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Expectations
{
	public interface IExpect
    {
        CheckResult Check();
        string FormatExpression(IFormatter<Expression> formatter);
        ConeMessage FormatMessage(IFormatter<object> formatter);
    }
}