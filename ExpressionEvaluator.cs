using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public class ExpressionEvaluator
    {
        static readonly Dictionary<KeyValuePair<Type, Type>, Func<object, object>> converters = new Dictionary<KeyValuePair<Type, Type>,Func<object,object>>();

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
                case ExpressionType.Quote: return EvaluateQuote(body as UnaryExpression, context);
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
            var parameters = new[]{ 
                Evaluate(binary.Left, context), 
                Evaluate(binary.Right, context) 
            };

            var op = binary.Method;
            if(op != null)
                return op.Invoke(null, parameters);
            return ExecuteAs<object>(binary);
        }

        static object EvaluateCall(MethodCallExpression expression, Expression context) {
            var target = EvaluateCallTarget(expression, context);
            var input = EvaluateAll(expression.Arguments, context);
            var method = expression.Method;

            try {
                return method.Invoke(target, input);
            } catch(TargetInvocationException e) {
                throw e.InnerException;
            } finally {
                AssignOutParameters(expression.Arguments, input, method.GetParameters());
            }
        }

        static void AssignOutParameters(IList<Expression> arguments, object[] results, ParameterInfo[] parameters) {
            if(results.Length == 0)
                return;
            for(int i = 0; i != parameters.Length; ++i)
                if(parameters[i].IsOut) {
                    var member = (arguments[i] as MemberExpression);
                    var field = member.Member as FieldInfo;                            
                    field.SetValue(Evaluate(member.Expression, member.Expression), results[i]);
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

            return GetConverter(expression)(source);
        }

        static Func<object, object> GetConverter(UnaryExpression conversion) {
            var key = ConverterKey(conversion);
            Func<object, object> converter;
            if(converters.TryGetValue(key, out converter)) 
                return converter;

            var input = Expression.Parameter(typeof(object), "input");
            return converters[key] = 
                    Expression.Lambda<Func<object, object>>(
                        Expression.Convert(
                            Expression.Convert(
                                Expression.Convert(input, key.Key), key.Value), typeof(object)), input).Compile();
        }

        static KeyValuePair<Type, Type> ConverterKey(UnaryExpression conversion) {
            return new KeyValuePair<Type,Type>(conversion.Operand.Type, conversion.Type);
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
                var args = EvaluateAll(expression.Arguments, context);
                if(expression.Constructor != null)
                    return expression.Constructor.Invoke(args);
                return Activator.CreateInstance(expression.Type, args);
            } catch(TargetInvocationException e) {
                throw e.InnerException;
            }
        }

        static object EvaluateQuote(UnaryExpression expression, Expression context) {
            return expression.Operand;
        }

        static object EvaluateAsTarget(Expression expression, Expression context) {
            if(expression == null)
                return null;
            var target = EvaluateAs<object>(expression);
            if(target == null)
                throw new NullSubexpressionException(context, expression);
            return target;
        }

        static object[] EvaluateAll(ICollection<Expression> expressions, Expression context) {
            var result = new object[expressions.Count];
            var index = 0;
            foreach(var item in expressions) {
                result[index++] = Evaluate(item, context);
            }
            return result;
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
