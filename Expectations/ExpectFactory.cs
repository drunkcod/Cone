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
        delegate Expect Expector<TValue>(BinaryExpression expression, TValue left, TValue right);

        static readonly Expector<object> EqualExpector = MakeExpector<object>(typeof(EqualExpect));     
        static readonly Expector<object> NotEqualExpector = MakeExpector<object>(typeof(NotEqualExpect));     
        static readonly Expector<object> BinaryExpector = MakeExpector<object>(typeof(BinaryExpect));
        static readonly Expector<object> LessThanExpector = MakeExpector<object>(typeof(LessThanExpect));     
        static readonly Expector<object> LessThanOrEqualExpector = MakeExpector<object>(typeof(LessThanOrEqualExpect));     
        static readonly Expector<object> GreaterThanExpector = MakeExpector<object>(typeof(GreaterThanExpect));     
        static readonly Expector<object> GreaterThanOrEqualExpector = MakeExpector<object>(typeof(GreaterThanOrEqualExpect));     
        static readonly Expector<string> StringEqualExpector = MakeExpector<string>(typeof(StringEqualExpect));
        static readonly ExpressionEvaluator Evaluator = new ExpressionEvaluator();

        static Expector<TValue> MakeExpector<TValue>(Type expectType) {
            var arguments = new[] { typeof(BinaryExpression), typeof(TValue), typeof(TValue) };
            var parameters = new[] {
                Expression.Parameter(arguments[0], "body"),
                Expression.Parameter(arguments[1], "left"),
                Expression.Parameter(arguments[2], "right")
            };
            return Expression.Lambda<Expector<TValue>>(
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
                var target = Evaluator.EvaluateAsTarget(m.Object, body).Value;
                return provider.GetExpectation(body, m.Method, target, m.Arguments.Select(EvaluateAs<object>));
            }
            return new BooleanExpect(body, EvaluateAs<bool>(body));
        }

        static Expect FromBinary(BinaryExpression body) {
            if(body.NodeType == ExpressionType.Equal) {
                if(body.Left.Type == typeof(string) && body.Right.Type == typeof(string))
                    return StringEqualExpector(body, EvaluateAs<string>(body.Left), EvaluateAs<string>(body.Right));
            }
            return GetExpector(body.NodeType)(body, EvaluateAs<object>(body.Left, body), EvaluateAs<object>(body.Right, body));
        }

        static Expector<object> GetExpector(ExpressionType op) {
            switch(op) {
                case ExpressionType.Equal: return EqualExpector;
                case ExpressionType.NotEqual: return NotEqualExpector;
                case ExpressionType.LessThan: return LessThanExpector;
                case ExpressionType.LessThanOrEqual: return LessThanOrEqualExpector;
                case ExpressionType.GreaterThan: return GreaterThanExpector;
                case ExpressionType.GreaterThanOrEqual: return GreaterThanOrEqualExpector;
            }
            return BinaryExpector;
        }
        
        static T EvaluateAs<T>(Expression body) { return (T)Evaluator.Evaluate(body, body).Value; }
        static T EvaluateAs<T>(Expression body, Expression context) { return (T)Evaluator.Evaluate(body, context).Value; }

        static Expect FromTypeIs(TypeBinaryExpression body) {
            var typeIs = (TypeBinaryExpression)body;
            return new TypeIsExpect(body,
                EvaluateAs<object>(typeIs.Expression).GetType(), 
                typeIs.TypeOperand);
        }
    }
}
