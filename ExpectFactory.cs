using System.Linq.Expressions;
using System;
using System.Reflection;

namespace Cone
{
    class ExpectFactory
    {
        static readonly ConstructorInfo BinaryExpectCtor = typeof(BinaryExpect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });
        static readonly ConstructorInfo ExpectCtor = typeof(Expect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });
        
        public IExpect From(Expression body) {
            if(body.NodeType == ExpressionType.Not)
                return new NotExpect(From(((UnaryExpression)body).Operand));

            if (SupportedExpressionType(body.NodeType))
                return Lambda(body);
            throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
        }

        static bool SupportedExpressionType(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Call: return true;
                case ExpressionType.Constant: return true;
                case ExpressionType.Equal: return true;
                case ExpressionType.NotEqual: return true;
                case ExpressionType.GreaterThan: return true;
                case ExpressionType.GreaterThanOrEqual: return true;
                case ExpressionType.LessThan: return true;
                case ExpressionType.LessThanOrEqual: return true;
                case ExpressionType.MemberAccess: return true;
                case ExpressionType.TypeIs: return true;
            }
            return false;
        }

        static Expect Lambda(Expression body) {
            var binary = body as BinaryExpression;
            if (binary != null)
                return FromBinary(binary);
            return FromSingle(body);
        }

        public static Expect FromSingle(Expression body) {
            return From(ExpectCtor, body, body, Expression.Constant(true));
        }

        public static Expect FromBinary(BinaryExpression body) {
            return From(BinaryExpectCtor, body, body.Left, body.Right);
        }

        
        static Expect From(ConstructorInfo ctor, Expression body, Expression left, Expression right) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(ctor,
                        Expression.Constant(body),
                        Box(left),
                        Box(right)))
                .Execute();
        }

        static Expression Box(Expression expression) { return Expression.TypeAs(expression, typeof(object)); }


    }
}
