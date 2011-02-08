using System.Linq.Expressions;

namespace Cone.Expectations
{
    public struct ExpectResult
    {
        public bool Success;
        public object Actual;
    }

    public interface IExpect
    {
        ExpectResult Check();
        string FormatExpression(IFormatter<Expression> formatter);
        string FormatMessage(IFormatter<object> formatter);
    }
}