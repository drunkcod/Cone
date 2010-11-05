using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Diagnostics;

namespace Cone.Expectations
{
    public class ExpectFactory
    {
        static readonly ConstructorInfo BinaryExpectCtor = typeof(BinaryExpect).GetConstructor(new[] { typeof(Expression), typeof(object), typeof(object) });
        static readonly ConstructorInfo StringEqualCtor = typeof(StringEqualExpect).GetConstructor(new[] { typeof(Expression), typeof(string), typeof(string) });
        static readonly ConstructorInfo EqualExpectCtor = typeof(EqualExpect).GetConstructor(new[]{ typeof(Expression), typeof(object), typeof(object) });     
        static readonly ConstructorInfo NotEqualExpectCtor = typeof(NotEqualExpect).GetConstructor(new[]{ typeof(Expression), typeof(object), typeof(object) });     
        static readonly ConstructorInfo LessThanExpectCtor = typeof(LessThanExpect).GetConstructor(new[]{ typeof(Expression), typeof(object), typeof(object) });     
        static readonly ConstructorInfo LessThanOrEqualExpectCtor = typeof(LessThanOrEqualExpect).GetConstructor(new[]{ typeof(Expression), typeof(object), typeof(object) });     
        static readonly ConstructorInfo GreaterThanExpectCtor = typeof(GreaterThanExpect).GetConstructor(new[]{ typeof(Expression), typeof(object), typeof(object) });     
        static readonly ConstructorInfo GreaterThanOrEqualExpectCtor = typeof(GreaterThanOrEqualExpect).GetConstructor(new[]{ typeof(Expression), typeof(object), typeof(object) });     
        
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
            if(body.NodeType == ExpressionType.TypeIs)
                return FromTypeIs((TypeBinaryExpression)body);
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
                    return From<object>(EqualExpectCtor, body, body.Left, body.Right);
            } else if(body.NodeType == ExpressionType.NotEqual) {
                    return From<object>(NotEqualExpectCtor, body, body.Left, body.Right);
            } else if(body.NodeType == ExpressionType.LessThan) {
                    return From<object>(LessThanExpectCtor, body, body.Left, body.Right);
            } else if(body.NodeType == ExpressionType.LessThanOrEqual) {
                    return From<object>(LessThanOrEqualExpectCtor, body, body.Left, body.Right);
            } else if(body.NodeType == ExpressionType.GreaterThan) {
                    return From<object>(GreaterThanExpectCtor, body, body.Left, body.Right);
            } else if(body.NodeType == ExpressionType.GreaterThanOrEqual) {
                    return From<object>(GreaterThanOrEqualExpectCtor, body, body.Left, body.Right);
            }

            return From<object>(BinaryExpectCtor, body, body.Left, body.Right);
        }
        
        static Expect From<T>(ConstructorInfo ctor, Expression body, Expression left, Expression right) {
            return Expression.New(ctor,
                        Expression.Constant(body),
                        left.CastTo<T>(),
                        right.CastTo<T>())
                .Execute<Expect>();
        }

        static Expect FromTypeIs(TypeBinaryExpression body) {
            var typeIs = (TypeBinaryExpression)body;
            return new EqualExpect(body,
                typeIs.Expression.CastTo<object>().Execute<object>().GetType(), 
                typeIs.TypeOperand);
        }
    }
}
