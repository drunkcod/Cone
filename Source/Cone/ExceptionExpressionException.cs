using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExceptionExpressionException : Exception
    {
        public readonly Expression Expression;
        public readonly Expression Context;

        public ExceptionExpressionException(Expression expression, Expression context, Exception innerException) : base("", innerException) {
            this.Expression = expression;
            this.Context = context;
        }
    }
}
