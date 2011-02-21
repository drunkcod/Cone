using System.Linq.Expressions;
using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Cone.Expectations
{
    public class ExpectFactory
    {
        delegate Expect Expector<TExpression, TValue>(TExpression expression, TValue left, TValue right);

        static readonly Expector<Expression, object> EqualExpector = MakeExpector<Expression, object>(typeof(EqualExpect));     
        static readonly Expector<Expression, object> NotEqualExpector = MakeExpector<Expression, object>(typeof(NotEqualExpect));     
        static readonly Expector<BinaryExpression, object> BinaryExpector = MakeExpector<BinaryExpression, object>(typeof(BinaryExpect));
        static readonly Expector<BinaryExpression, object> LessThanExpector = MakeExpector<BinaryExpression, object>(typeof(LessThanExpect));     
        static readonly Expector<BinaryExpression, object> LessThanOrEqualExpector = MakeExpector<BinaryExpression, object>(typeof(LessThanOrEqualExpect));     
        static readonly Expector<BinaryExpression, object> GreaterThanExpector = MakeExpector<BinaryExpression, object>(typeof(GreaterThanExpect));     
        static readonly Expector<BinaryExpression, object> GreaterThanOrEqualExpector = MakeExpector<BinaryExpression, object>(typeof(GreaterThanOrEqualExpect));     
        static readonly Expector<Expression, string> StringEqualExpector = MakeExpector<Expression, string>(typeof(StringEqualExpect));

        static ConstructorInfo GetExpectCtor<T>(Type expectType) {
            return expectType.GetConstructor(new[]{ typeof(T), typeof(object), typeof(object) });
        }

        static Expector<TExpression, TValue> MakeExpector<TExpression, TValue>(Type expectType) {
            var arguments = new[] { typeof(TExpression), typeof(TValue), typeof(TValue) };
            var parameters = new[] {
                Expression.Parameter(arguments[0], "body"),
                Expression.Parameter(arguments[1], "left"),
                Expression.Parameter(arguments[2], "right")
            };
            return Expression.Lambda<Expector<TExpression, TValue>>(
                Expression.New(expectType.GetConstructor(arguments), parameters), parameters).Compile();
        }

        readonly IDictionary<MethodInfo, IMethodExpectProvider> methodExpects = new Dictionary<MethodInfo, IMethodExpectProvider>();

        public ExpectFactory() {
            var providers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(IMethodExpectProvider).IsAssignableFrom(x) && x.IsClass)
                .Select(x => x.GetConstructor(Type.EmptyTypes).Invoke(null) as IMethodExpectProvider);
            foreach(var provider in providers)
                foreach(var method in provider.GetSupportedMethods())
                    methodExpects[method] = provider;
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

        IExpect Lambda(Expression body) {
            var binary = body as BinaryExpression;
            if (binary != null)
                return FromBinary(binary);
            if(body.NodeType == ExpressionType.TypeIs)
                return FromTypeIs((TypeBinaryExpression)body);
            return FromSingle(body);
        }

        IExpect FromSingle(Expression body) {
            IMethodExpectProvider provider;
            if(body.NodeType == ExpressionType.Call && methodExpects.TryGetValue(((MethodCallExpression)body).Method, out provider)) {
                var m = (MethodCallExpression)body;
                return provider.GetExpectation(body, m.Method, Expression.Lambda<Func<object>>(m.Object).Execute(), m.Arguments.Select(x => Expression.Lambda<Func<object>>(x).Execute()).ToArray());
            }
            return new BooleanExpect(body, EvaluateAs<bool>(body));
        }

        static Expect FromBinary(BinaryExpression body) {
            if(body.NodeType == ExpressionType.Equal) {
                if(body.Left.Type == typeof(string) && body.Right.Type == typeof(string))
                    return StringEqualExpector(body, EvaluateAs<string>(body.Left), EvaluateAs<string>(body.Right));
            }
            return MakeExpect(body, EvaluateAs<object>(body.Left), EvaluateAs<object>(body.Right));
        }

        static Expect MakeExpect(Expression body, object left, object right) {
            switch(body.NodeType) {
                case ExpressionType.Equal: return EqualExpector(body, left, right);
                case ExpressionType.NotEqual: return NotEqualExpector(body, left, right);
                case ExpressionType.LessThan: return LessThanExpector(body as BinaryExpression, left, right);
                case ExpressionType.LessThanOrEqual: return LessThanOrEqualExpector(body as BinaryExpression, left, right);
                case ExpressionType.GreaterThan: return GreaterThanExpector(body as BinaryExpression, left, right);
                case ExpressionType.GreaterThanOrEqual: return GreaterThanOrEqualExpector(body as BinaryExpression, left, right);
            }
            return BinaryExpector(body as BinaryExpression, left, right);
        }
        
        static T EvaluateAs<T>(Expression body) {
            switch(body.NodeType) {
                case ExpressionType.Constant: return (T)(body as ConstantExpression).Value;
                default: return body.ExecuteAs<T>();
            }
        }

        static Expect FromTypeIs(TypeBinaryExpression body) {
            var typeIs = (TypeBinaryExpression)body;
            return new TypeIsExpect(body,
                EvaluateAs<object>(typeIs.Expression).GetType(), 
                typeIs.TypeOperand);
        }
    }
}
