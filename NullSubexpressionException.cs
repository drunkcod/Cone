using System;
using System.Linq.Expressions;

namespace Cone
{
    public class NullSubexpressionException : ArgumentNullException 
    {
        readonly Expression expression;
        readonly Expression nullSubexpression;

        public NullSubexpressionException(Expression expression, Expression nullSubexpression) {
            this.expression = expression;
            this.nullSubexpression = nullSubexpression;
        }

        public Expression Expression { get { return expression; } }
        public Expression NullSubexpression { get { return nullSubexpression; } }
    }
}
