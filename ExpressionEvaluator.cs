using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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
        public static T Evaluate<T>(Expression<Func<T>> lambda) { return EvaluateAs<T>(lambda.Body); }

        public static T EvaluateAs<T>(Expression body) {
            switch(body.NodeType) {
                case ExpressionType.Equal: goto case ExpressionType.NotEqual;
                case ExpressionType.NotEqual: return EvaluateAs<T>((BinaryExpression)body);
                case ExpressionType.Call: return (T)EvaluateCall(body as MethodCallExpression, body);
                case ExpressionType.Constant: return (T)(body as ConstantExpression).Value;
                case ExpressionType.MemberAccess: return (T)EvaluateMemberAccess(body as MemberExpression, body);
                default: return ExecuteAs<T>(body);
            }
        }

        public static object EvaluateCallTarget(MethodCallExpression expression, Expression context) {
            if(expression.Object == null)
                return null;
            var target = EvaluateAs<object>(expression.Object);
            if(target == null)
                throw new NullSubexpressionException(context, expression.Object);
            return target;
        }

        static T EvaluateAs<T>(BinaryExpression binary) {
            var left = EvaluateAs<object>(binary.Left);
            var right = EvaluateAs<object>(binary.Right);
            return (T)binary.Method.Invoke(null, new[]{ left, right });
        }

        static T ExecuteAs<T>(Expression body) { return body.CastTo<T>().Execute<T>(); }

        static object EvaluateCall(MethodCallExpression expression, Expression context) {
            object target = EvaluateCallTarget(expression, context);
            try {
                return expression.Method.Invoke(target, EvaluateAll(expression.Arguments));
            } catch(TargetInvocationException e) {
                throw e.InnerException;
            }
        }

        static object EvaluateMemberAccess(MemberExpression expression, Expression context) {
            object target = null;
            if(expression.Expression != null) {
                target = EvaluateAs<object>(expression.Expression);
                if(target == null)
                    throw new NullSubexpressionException(context, expression.Expression);
            }
            try {
                return GetValue(target, expression.Member);
            } catch(TargetInvocationException e) {
                throw e.InnerException;
            }
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
