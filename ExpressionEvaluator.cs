using System;
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
                case ExpressionType.Call: return (T)EvaluateCall(body, body as MethodCallExpression);
                case ExpressionType.Constant: return (T)(body as ConstantExpression).Value;
                case ExpressionType.MemberAccess: return (T)EvaluateMemberAccess(body, body as MemberExpression);
                default: return ExecuteAs<T>(body);
            }
        }

        public static object EvaluateCallTarget(Expression body, MethodCallExpression expression) {
            if(expression.Object == null)
                return null;
            var target = EvaluateAs<object>(expression.Object);
            if(target == null)
                throw new NullSubexpressionException(body, expression.Object);
            return target;
        }

        static T EvaluateAs<T>(BinaryExpression binary) {
            var left = EvaluateAs<object>(binary.Left);
            var right = EvaluateAs<object>(binary.Right);
            return (T)binary.Method.Invoke(null, new[]{ left, right });
        }

        static T ExecuteAs<T>(Expression body) { return body.CastTo<T>().Execute<T>(); }

        static object EvaluateCall(Expression body, MethodCallExpression expression) {
            object target = EvaluateCallTarget(body, expression);
            return ExecuteAs<object>(expression);
        }

        static object EvaluateMemberAccess(Expression body, MemberExpression expression) {
            object target = null;
            if(expression.Expression != null) {
                target = EvaluateAs<object>(expression.Expression);
                if(target == null)
                    throw new NullSubexpressionException(body, expression.Expression);
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
    }
}
