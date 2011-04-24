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
                .Where(IsMethodExpectProvider)
                .Select(x => x.GetConstructor(Type.EmptyTypes).Invoke(null) as IMethodExpectProvider);
            foreach(var provider in providers)
                foreach(var method in provider.GetSupportedMethods())
                    methodExpects[method] = provider;
        }

        static Func<Type, bool> ImplementsIMethodExpectProvider = typeof(IMethodExpectProvider).IsAssignableFrom;
        static bool IsMethodExpectProvider(Type type) {
            return type.IsPublic && type.IsClass && ImplementsIMethodExpectProvider(type);
        }

        public IExpect From(Expression body) {
            if(body.NodeType == ExpressionType.Not)
                return new NotExpect(From(((UnaryExpression)body).Operand));

            if(body.NodeType == ExpressionType.AndAlso)
                return new BooleanExpect(body, Evaluate(body, body));

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
            if(body.NodeType == ExpressionType.Call)
                return FromCall((MethodCallExpression)body);
            return Boolean(body);
        }

        IExpect FromCall(MethodCallExpression body) {
            IMethodExpectProvider provider;
            var method = body.Method;
            if(methodExpects.TryGetValue(method, out provider)) {
                var target = Evaluator.EvaluateAsTarget(body.Object, body).Value;
                return provider.GetExpectation(body, method, target, body.Arguments.Select(EvaluateAs<object>));
            }
            return Boolean(body);;
        }

        IExpect Boolean(Expression body) {
            return new BooleanExpect(body, EvaluateAs<bool>(body));
        }

        static Expect FromBinary(BinaryExpression body) {
            var left = Evaluate(body.Left, body);
            var right = Evaluate(body.Right, body);

            if(body.NodeType == ExpressionType.Equal
            && body.Left.Type == typeof(string) 
            && body.Right.Type == typeof(string)) {
                return StringEqualExpector(body, (string)left, (string)right);
            }
            return GetExpector(body.NodeType)(body, left, right);
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
        
        static T EvaluateAs<T>(Expression body) { return (T)Evaluate(body, body); }
        static object Evaluate(Expression body, Expression context) { return Evaluator.Evaluate(body, context).Value; }

        static Expect FromTypeIs(TypeBinaryExpression body) {
            return new TypeIsExpect(body,
                Evaluate(body.Expression, body).GetType(), 
                body.TypeOperand);
        }
    }
}
