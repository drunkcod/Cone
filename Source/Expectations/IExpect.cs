using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Expectations
{
    public struct ExpectResult
    {
        public bool Success;
        public object Actual;

        public static bool operator==(ExpectResult left, ExpectResult right) {
            return left.Equals(right);
        }

        public static bool operator!=(ExpectResult left, ExpectResult right) {
            return !(left == right);
        }

        public override string ToString() {
            return string.Format("{{Success: {0}, Actual: {1}}}", Success, Actual);
        }
    }

    public interface IExpect
    {
        ExpectResult Check();
        string FormatExpression(IFormatter<Expression> formatter);
        string FormatMessage(IFormatter<object> formatter);
    }
}