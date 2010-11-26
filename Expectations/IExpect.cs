using System.Linq.Expressions;

namespace Cone.Expectations
{
    public interface IExpect
    {
        bool Check(out object actual);
        string FormatExpression(IFormatter<Expression> formatter);
        string FormatMessage(IFormatter<object> formatter);
    }
}