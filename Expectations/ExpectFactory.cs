using System.Linq.Expressions;
using System;
using System.Reflection;

namespace Cone.Expectations
{
    public class ExpectFactory
    {
        static readonly ConstructorInfo BinaryExpectCtor = typeof(BinaryExpect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });
        static readonly ConstructorInfo BinaryEqualExpectCtor = typeof(EqualExpect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });
        static readonly ConstructorInfo StringEqualCtor = typeof(StringEqualExpect).GetConstructor(new[] { typeof(Expression), typeof(string), typeof(string) });
     
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

        static IExpect Lambda(Expression body) {
            var binary = body as BinaryExpression;
            if (binary != null)
                return FromBinary(binary);
            return FromSingle(body);
        }

        static IExpect FromSingle(Expression body) {
            return new BooleanExpect(body, Expression.Lambda<Func<bool>>(body).Execute());
        }

        static Expect FromBinary(BinaryExpression body) {
            if(body.NodeType == ExpressionType.Equal) {
                if(body.Left.Type == typeof(string) && body.Right.Type == typeof(string))
                    return From<string>(StringEqualCtor, body, body.Left, body.Right);
                else 
                    return From<object>(BinaryEqualExpectCtor, body, body.Left, body.Right);
            }            
            return From<object>(BinaryExpectCtor, body, body.Left, body.Right);
        }

        
        static Expect From<T>(ConstructorInfo ctor, Expression body, Expression left, Expression right) {
            return Expression.Lambda<Func<Expect>>(
                Expression.New(ctor,
                        Expression.Constant(body),
                        Cast<T>(left),
                        Cast<T>(right)))
                .Execute();
        }

        static Expression Cast<T>(Expression expression) { return Expression.TypeAs(expression, typeof(T)); }
    }
}
