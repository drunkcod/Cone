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
                
                case ExpressionType.Constant: return (T)(body as ConstantExpression).Value;
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)body;
                    if(member.Expression == null)
                        goto default;
                    var target = EvaluateAs<object>(member.Expression);
                    if(target == null)
                        throw new NullSubexpressionException(body, member.Expression);
                    try {
                        switch(member.Member.MemberType) {
                            case MemberTypes.Field: 
                                var field = member.Member as FieldInfo;
                                return (T)field.GetValue(target);
                            case MemberTypes.Property:
                                var prop = member.Member as PropertyInfo;
                                return (T)prop.GetValue(target, null);
                            default: throw new NotSupportedException();
                        }
                    } catch(TargetInvocationException invocationException) {
                        throw invocationException.InnerException;
                    }
                default: return ExecuteAs<T>(body);
            }
        }

        static T EvaluateAs<T>(BinaryExpression binary) {
            var left = EvaluateAs<object>(binary.Left);
            var right = EvaluateAs<object>(binary.Right);
            return (T)binary.Method.Invoke(null, new[]{ left, right });
        }

        static T ExecuteAs<T>(Expression body) { return body.CastTo<T>().Execute<T>(); }
    }
}
