﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

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

    public class ExpressionEvaluator
    {
        static readonly Dictionary<Type, Func<object, object>> converters = new Dictionary<Type,Func<object,object>>();

        public static T Evaluate<T>(Expression<Func<T>> lambda) { return EvaluateAs<T>(lambda.Body); }

        public static T EvaluateAs<T>(Expression body) { return (T)Evaluate(body, body); }

        static object Evaluate(Expression body, Expression context) {
            switch(body.NodeType) {
                case ExpressionType.Call: return EvaluateCall(body as MethodCallExpression, context);
                case ExpressionType.Constant: return (body as ConstantExpression).Value;
                case ExpressionType.Convert: return EvaluateConvert(body as UnaryExpression, context);
                case ExpressionType.Equal: goto case ExpressionType.NotEqual;
                case ExpressionType.NotEqual: return EvaluateBinary(body as BinaryExpression, context);
                case ExpressionType.MemberAccess: return EvaluateMemberAccess(body as MemberExpression, context);
                case ExpressionType.New: return EvaluateNew(body as NewExpression, context);
                default: return ExecuteAs<object>(body);
            }
        }

        public static object EvaluateCallTarget(MethodCallExpression expression, Expression context) {
            return EvaluateAsTarget(expression.Object, context);
        }

        static T ExecuteAs<T>(Expression body) {
            return body.CastTo<T>().Execute<T>(); 
        }

        static object EvaluateBinary(BinaryExpression binary, Expression context) {
            return binary.Method.Invoke(null, new[]{ 
                Evaluate(binary.Left, context), 
                Evaluate(binary.Right, context) });
        }

        static object EvaluateCall(MethodCallExpression expression, Expression context) {
            object target = EvaluateCallTarget(expression, context);
            try {
                return expression.Method.Invoke(target, EvaluateAll(expression.Arguments));
            } catch(TargetInvocationException e) {
                throw e.InnerException;
            }
        }

        static object EvaluateConvert(UnaryExpression expression, Expression context) {
            var source = Evaluate(expression.Operand, context);
            var convertMethod = expression.Method;
            if(convertMethod != null && convertMethod.IsStatic) {
                try {
                    return convertMethod.Invoke(null, new[] { source });
                } catch(TargetInvocationException e) {
                    throw e.InnerException;
                }
            }

            Func<object, object> converter;
            if(!converters.TryGetValue(expression.Type, out converter)) {
                var input = Expression.Parameter(typeof(object), "input");
                converters[expression.Type] = converter = Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.Convert(input, expression.Type), typeof(object)), input).Compile();
            }
            
            return converter(source);
        }

        static object EvaluateMemberAccess(MemberExpression expression, Expression context) {
            try {
                return GetValue(EvaluateAsTarget(expression.Expression, context), expression.Member);
            } catch(TargetInvocationException e) {
                throw e.InnerException;
            }
        }

        static object EvaluateNew(NewExpression expression, Expression context) {
            try {
                return expression.Constructor.Invoke(EvaluateAll(expression.Arguments));
            } catch(TargetInvocationException e) {
                throw e.InnerException;
            }
        }

        static object EvaluateAsTarget(Expression expression, Expression context) {
            if(expression == null)
                return null;
            var target = EvaluateAs<object>(expression);
            if(target == null)
                throw new NullSubexpressionException(context, expression);
            return target;
        }

        static object GetValue(object target, MemberInfo member) {
            switch(member.MemberType) {
                case MemberTypes.Field: 
                    return (member as FieldInfo).GetValue(target);
                case MemberTypes.Property:
                    return (member as PropertyInfo).GetValue(target, null);
                default: throw new NotSupportedException();
            }
        }

        static object[] EvaluateAll(ICollection<Expression> expressions) {
            var result = new object[expressions.Count];
            var index = 0;
            foreach(var item in expressions)
                result[index++] = EvaluateAs<object>(item);
            return result;
        }
    }
}
