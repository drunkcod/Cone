﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Expectations
{
    public class ExpectFactory
    {
        delegate Expect Expector(BinaryExpression body, object left, object right);

        static readonly Expector EqualExpector = MakeExpector(typeof(EqualExpect));     
        static readonly Expector NotEqualExpector = MakeExpector(typeof(NotEqualExpect));     
        static readonly Expector BinaryExpector = MakeExpector(typeof(BinaryExpect));
        static readonly Expector LessThanExpector = MakeExpector(typeof(LessThanExpect));     
        static readonly Expector LessThanOrEqualExpector = MakeExpector(typeof(LessThanOrEqualExpect));     
        static readonly Expector GreaterThanExpector = MakeExpector(typeof(GreaterThanExpect));     
        static readonly Expector GreaterThanOrEqualExpector = MakeExpector(typeof(GreaterThanOrEqualExpect));
        static readonly ExpressionEvaluator Evaluator = new ExpressionEvaluator();

        static Expector MakeExpector(Type expectType) {
            var arguments = new[] { typeof(BinaryExpression), typeof(object), typeof(object) };
            var parameters = new[] {
                Expression.Parameter(arguments[0], "body"),
                Expression.Parameter(arguments[1], "left"),
                Expression.Parameter(arguments[2], "right")
            };
            return Expression.Lambda<Expector>(Expression.New(expectType.GetConstructor(arguments), parameters), parameters).Compile();
        }

        readonly IDictionary<MethodInfo, IMethodExpectProvider> methodExpects = new Dictionary<MethodInfo, IMethodExpectProvider>();

        public ExpectFactory() {
            var providers = AppDomain.CurrentDomain.GetAssemblies()                
                .SelectMany(x => x.GetTypes())
                .Where(IsMethodExpectProvider)
                .Select(x => x.New() as IMethodExpectProvider);
            foreach(var provider in providers)
                foreach(var method in provider.GetSupportedMethods())
                    methodExpects[method] = provider;
        }

        public static bool IsMethodExpectProvider(Type type) {
            return type.IsVisible && type.IsClass && type.Implements<IMethodExpectProvider>();
        }

        public IExpect From(Expression body) {
            if(body.NodeType == ExpressionType.Not)
                return new NotExpect(From(((UnaryExpression)body).Operand));

            if(body.NodeType == ExpressionType.AndAlso)
                return Boolean(body);

            if(body.NodeType == ExpressionType.Convert) {
                var conversion = (UnaryExpression)body;
                if(conversion.Type == typeof(bool))
                    return Boolean(body);
            }

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
                return Binary(binary);
            if(body.NodeType == ExpressionType.TypeIs)
                return TypeIs((TypeBinaryExpression)body);
            return Unary(body);
        }

        IExpect Unary(Expression body) {
            if(body.NodeType == ExpressionType.Call)
                return Method((MethodCallExpression)body);
            return Boolean(body);
        }

        IExpect Method(MethodCallExpression body) {
            IMethodExpectProvider provider;
            var method = body.Method;
            if(methodExpects.TryGetValue(method, out provider)) {
                var target = Evaluator.EvaluateAsTarget(body.Object, body).Value;
                var args = body.Arguments.ConvertAll(EvaluateAs<object>);
                return provider.GetExpectation(body, method, target, args);
            }
            return Boolean(body);
        }

        IExpect Boolean(Expression body) {
            return new BooleanExpect(body, EvaluateAs<bool>(body));
        }

        static Expect Binary(BinaryExpression body) {
            var left = Evaluate(body.Left, body);
            var right = Evaluate(body.Right, body);

            if(body.NodeType == ExpressionType.Equal
            && body.Left.Type == typeof(string) 
            && body.Right.Type == typeof(string)) {
                return new StringEqualExpect(body, (string)left, (string)right);
            }
            return GetExpector(body.NodeType)(body, left, right);
        }

        static Expector GetExpector(ExpressionType op) {
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

        static Expect TypeIs(TypeBinaryExpression body) {
            return new TypeIsExpect(body,
                Evaluate(body.Expression, body).GetType(), 
                body.TypeOperand);
        }
    }
}
