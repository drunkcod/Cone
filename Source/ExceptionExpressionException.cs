using System;
using System.Linq.Expressions;

namespace Cone
{
    public class ExceptionExpressionException : Exception
    {
        public readonly Expression Expression;
        public readonly Expression Subexpression;

        public ExceptionExpressionException(Expression expression, Expression subexpression, Exception innerException) : base("", innerException) {
            this.Expression = expression;
            this.Subexpression = subexpression;
        }
    }
}
