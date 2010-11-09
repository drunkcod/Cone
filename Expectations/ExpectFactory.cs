using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Diagnostics;

namespace Cone.Expectations
{
    public class ExpectFactory
    {
        static readonly ConstructorInfo BinaryExpectCtor = GetExpectCtor(typeof(BinaryExpect));
        static readonly ConstructorInfo EqualExpectCtor = GetExpectCtor(typeof(EqualExpect));     
        static readonly ConstructorInfo NotEqualExpectCtor = GetExpectCtor(typeof(NotEqualExpect));     
        static readonly ConstructorInfo LessThanExpectCtor = GetExpectCtor(typeof(LessThanExpect));     
        static readonly ConstructorInfo LessThanOrEqualExpectCtor = GetExpectCtor(typeof(LessThanOrEqualExpect));     
        static readonly ConstructorInfo GreaterThanExpectCtor = GetExpectCtor(typeof(GreaterThanExpect));     
        static readonly ConstructorInfo GreaterThanOrEqualExpectCtor = GetExpectCtor(typeof(GreaterThanOrEqualExpect));     
        static readonly ConstructorInfo StringEqualCtor = typeof(StringEqualExpect).GetConstructor(new[]{ typeof(Expression), typeof(string), typeof(string) });

        static ConstructorInfo GetExpectCtor(Type expectType) {
            return expectType.GetConstructor(new[]{ typeof(Expression), typeof(object), typeof(object) });
        }

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
            }
            return From<object>(GetBinaryExpectCtor(body.NodeType), body, body.Left, body.Right);
        }

        static ConstructorInfo GetBinaryExpectCtor(ExpressionType op) {
            switch(op) {
                case ExpressionType.Equal: return EqualExpectCtor;
                case ExpressionType.NotEqual: return NotEqualExpectCtor;
                case ExpressionType.LessThan: return LessThanExpectCtor;
                case ExpressionType.LessThanOrEqual: return LessThanOrEqualExpectCtor;
                case ExpressionType.GreaterThan: return GreaterThanExpectCtor;
                case ExpressionType.GreaterThanOrEqual: return GreaterThanOrEqualExpectCtor;
            }

            return BinaryExpectCtor;
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
            return new TypeIsExpect(body,
                typeIs.Expression.CastTo<object>().Execute<object>().GetType(), 
                typeIs.TypeOperand);
        }
    }
}
