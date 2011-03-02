using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    public class UnsupportedExpressionException : Exception
    {
        public readonly Expression Expression;

        public UnsupportedExpressionException(Expression expression) {
            this.Expression = expression;
        }
    }

    public class ExceptionExpressionException : Exception
    {
        public readonly Expression Expression;
        public readonly Expression Subexpression;

        public ExceptionExpressionException(Expression expression, Expression subexpression, Exception innerException) : base("", innerException) {
            this.Expression = expression;
            this.Subexpression = subexpression;
        }
    }

    public class ExpressionEvaluator
    {
        static readonly Dictionary<KeyValuePair<Type, Type>, Func<object, object>> converters = new Dictionary<KeyValuePair<Type, Type>,Func<object,object>>();

        static readonly ExpressionEvaluator defaultEvaluator = new ExpressionEvaluator();

        public Func<Expression,object> Unsupported = x => Expression.Lambda<Func<object>>(Expression.Convert(x, typeof(object))).Compile()(); 
        
        public static T Evaluate<T>(Expression<Func<T>> lambda) { return EvaluateAs<T>(lambda.Body); }

        public static T EvaluateAs<T>(Expression body) { return EvaluateAs<T>(body, body); } 

        public static T EvaluateAs<T>(Expression body, Expression context) { 
            return EvaluateAs<T>(body, context, x => { throw new ExceptionExpressionException(body, context, x); });
        }

        public static T EvaluateAs<T>(Expression body, Expression context, Action<Exception> onError) { 
            return defaultEvaluator.EvaluateCore<T>(body, context, onError);
        }

        public T EvaluateCore<T>(Expression body, Expression context, Action<Exception> onError) { 
            try {
                return (T)Evaluate(body, context); 
            } catch(Exception e) {
                onError(e);
                return default(T);
            }
        }

        object Evaluate(Expression body, Expression context) {
            switch(body.NodeType) {
                case ExpressionType.Lambda: return EvaluateLambda(body as LambdaExpression, context);
                case ExpressionType.ArrayIndex: return EvaluateArrayIndex(body, context);
                case ExpressionType.Call: return EvaluateCall(body as MethodCallExpression, context);
                case ExpressionType.Constant: return (body as ConstantExpression).Value;
                case ExpressionType.Convert: return EvaluateConvert(body as UnaryExpression, context);
                case ExpressionType.Equal: goto case ExpressionType.NotEqual;
                case ExpressionType.NotEqual: return EvaluateBinary(body as BinaryExpression, context);
                case ExpressionType.MemberAccess: return EvaluateMemberAccess(body as MemberExpression, context);
                case ExpressionType.New: return EvaluateNew(body as NewExpression, context);
                case ExpressionType.Quote: return EvaluateQuote(body as UnaryExpression, context);
                case ExpressionType.Invoke: return EvaluateInvoke(body as InvocationExpression, context);
                default: return Unsupported(body);
            }
        }

        public static object EvaluateCallTarget(MethodCallExpression expression, Expression context) {
            return EvaluateAsTarget(expression.Object, context);
        }

        object EvaluateLambda(LambdaExpression expression, Expression context) {
            if(expression.Parameters.Count != 0)
                return Unsupported(expression);
            return Evaluate(expression.Body, context);
        }

        object EvaluateArrayIndex(Expression expression, Expression context) {
            var rank1 = expression as BinaryExpression;
            if(rank1 != null) {
                var array = (Array)Evaluate(rank1.Left, context);
                var index = (int)Evaluate(rank1.Right, context);
                return array.GetValue(index);
            }
            return Unsupported(expression);
        }

        object EvaluateBinary(BinaryExpression binary, Expression context) {
            var parameters = new[]{ 
                Evaluate(binary.Left, context), 
                Evaluate(binary.Right, context) 
            };

            var op = binary.Method;
            if(op != null)
                return op.Invoke(null, parameters);
            switch(binary.NodeType) {
                case ExpressionType.Equal: return Object.Equals(parameters[0], parameters[1]);
                default: return Unsupported(binary);
            }
        }

        object EvaluateCall(MethodCallExpression expression, Expression context) {
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

        void AssignOutParameters(IList<Expression> arguments, object[] results, ParameterInfo[] parameters) {
            if(results.Length == 0)
                return;
            for(int i = 0; i != parameters.Length; ++i)
                if(parameters[i].IsOut) {
                    var member = (arguments[i] as MemberExpression);
                    var field = member.Member as FieldInfo;                            
                    field.SetValue(Evaluate(member.Expression, member.Expression), results[i]);
                }
        }

        object EvaluateConvert(UnaryExpression expression, Expression context) {
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

        object EvaluateNew(NewExpression expression, Expression context) {
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

        object EvaluateInvoke(InvocationExpression expression, Expression context) {
            var target = Evaluate(expression.Expression, context) as Delegate;
            try {
                return target.DynamicInvoke(EvaluateAll(expression.Arguments, context));
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

        object[] EvaluateAll(ICollection<Expression> expressions, Expression context) {
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
