using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

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

    public struct EvaluationResult 
    {
        object value;
        bool isError;

        public static EvaluationResult Failure(Exception e){ return new EvaluationResult { value = e, isError = true }; }
        public static EvaluationResult Success(object result){ return new EvaluationResult { value = result, isError = false }; }

        public object Value { 
            get {
                if(IsError)
                    throw (Exception)value;
                return value; 
            } 
        }
        public Exception Error { get { return (Exception)value; } }
        public bool IsError { get { return isError; } }
    }

    public class ExpressionEvaluator
    {
        static readonly Dictionary<KeyValuePair<Type, Type>, Func<object, object>> converters = new Dictionary<KeyValuePair<Type, Type>,Func<object,object>>();

        static readonly ExpressionEvaluator defaultEvaluator = new ExpressionEvaluator();

        static Exception PreserveStackTrace(Exception e) {
            var context = new StreamingContext(StreamingContextStates.CrossAppDomain);
            var mgr = new ObjectManager(null, context) ;
            var si  = new SerializationInfo(e.GetType (), new FormatterConverter());

            e.GetObjectData(si, context) ;
            mgr.RegisterObject(e, 1, si);
            mgr.DoFixups();

            return e;
        }

        public ExpressionEvaluator() {
            Unsupported = x => EvaluateUnsupported(x); 
        }

        public Func<Expression,EvaluationResult> Unsupported; 
        
        public static T Evaluate<T>(Expression<Func<T>> lambda) { return (T)defaultEvaluator.Evaluate(lambda.Body, lambda).Value; }

        public static T EvaluateAs<T>(Expression body) { return (T)defaultEvaluator.Evaluate(body, body).Value; } 

        public EvaluationResult Evaluate(Expression body, Expression context) { 
            return Evaluate(body, context, x => { throw new ExceptionExpressionException(body, context, x.Error); });
        }

        public EvaluationResult Evaluate(Expression body, Expression context, Func<EvaluationResult, EvaluationResult> onError) {
            var result = EvaluateCore(body, context);
            if(result.IsError)
                return onError(result);
            return result;
        }

        EvaluationResult EvaluateCore(Expression body, Expression context) {
            switch(body.NodeType) {
                case ExpressionType.Lambda: return EvaluateLambda(body as LambdaExpression, context);
                case ExpressionType.ArrayIndex: return EvaluateArrayIndex(body, context);
                case ExpressionType.Call: return EvaluateCall(body as MethodCallExpression, context);
                case ExpressionType.Constant: return Success((body as ConstantExpression).Value);
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

        EvaluationResult EvaluateLambda(LambdaExpression expression, Expression context) {
            if(expression.Parameters.Count != 0)
                return Unsupported(expression);
            return EvaluateCore(expression.Body, context);
        }

        EvaluationResult EvaluateArrayIndex(Expression expression, Expression context) {
            var rank1 = expression as BinaryExpression;
            if(rank1 != null) {
                var array = (Array)EvaluateCore(rank1.Left, context).Value;
                var index = (int)EvaluateCore(rank1.Right, context).Value;
                return Success(array.GetValue(index));
            }
            return Unsupported(expression);
        }

        EvaluationResult EvaluateBinary(BinaryExpression binary, Expression context) {
            var left = EvaluateCore(binary.Left, context);
            if(left.IsError)
                return left;

            var right = EvaluateCore(binary.Right, context);
            if(right.IsError)
                return right;

            var parameters = new[] { 
                left.Value, 
                right.Value
            };

            var op = binary.Method;
            if(op != null)
                return Success(op.Invoke(null, parameters));
            switch(binary.NodeType) {
                case ExpressionType.Equal: return Success(Object.Equals(parameters[0], parameters[1]));
                default: return Unsupported(binary);
            }
        }

        EvaluationResult EvaluateCall(MethodCallExpression expression, Expression context) {
            var target = EvaluateAsTarget(expression.Object, context);
            if(target.IsError)
                return target;
            var input = EvaluateAll(expression.Arguments, context).Value as object[];
            var method = expression.Method;

            try {
                return Success(method.Invoke(target.Value, input));
            } catch(TargetInvocationException e) {
                return Failure(e.InnerException);
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
                    field.SetValue(EvaluateCore(member.Expression, member.Expression).Value, results[i]);
                }
        }

        EvaluationResult EvaluateConvert(UnaryExpression expression, Expression context) {
            var source = EvaluateCore(expression.Operand, context).Value;
            var convertMethod = expression.Method;
            if(convertMethod != null && convertMethod.IsStatic) {
                try {
                    return Success(convertMethod.Invoke(null, new[] { source }));
                } catch(TargetInvocationException e) {
                    return Failure(e.InnerException);
                }
            }
            var value = GetConverter(expression)(source);
            return Success(value);
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

        EvaluationResult EvaluateMemberAccess(MemberExpression expression, Expression context) {
            try {
                var target = EvaluateAsTarget(expression.Expression, context);
                if(target.IsError)
                    return target;
                return Success(GetValue(target.Value, expression.Member));
            } catch(TargetInvocationException e) {
                return Failure(e.InnerException);
            }
        }

        EvaluationResult EvaluateNew(NewExpression expression, Expression context) {
            try {
                var args = EvaluateAll(expression.Arguments, context).Value as object[];
                if(expression.Constructor != null)
                    return Success(expression.Constructor.Invoke(args));
                return Success(Activator.CreateInstance(expression.Type, args));
            } catch(TargetInvocationException e) {
                return Failure(e.InnerException);
            }
        }

        EvaluationResult EvaluateQuote(UnaryExpression expression, Expression context) {
            return Success(expression.Operand);
        }

        EvaluationResult EvaluateInvoke(InvocationExpression expression, Expression context) {
            var target = EvaluateCore(expression.Expression, context).Value as Delegate;
            try {
                var args = EvaluateAll(expression.Arguments, context).Value as object[];
                return Success(target.DynamicInvoke(args));
            } catch(TargetInvocationException e) {
                return Failure(e.InnerException);
            }
        }

        public EvaluationResult EvaluateAsTarget(Expression expression, Expression context) {
            if(expression == null)
                return Success(null);
            var target = EvaluateCore(expression, context);
            if(target.IsError || target.Value == null)
                return Failure(new NullSubexpressionException(context, expression));
            return target;
        }

        EvaluationResult EvaluateAll(ICollection<Expression> expressions, Expression context) {
            var result = new object[expressions.Count];
            var index = 0;
            foreach(var item in expressions) {
                result[index++] = EvaluateCore(item, context).Value;
            }
            return Success(result);
        }

        EvaluationResult EvaluateUnsupported(Expression expression) {
            try {
                return Success(Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile()());
            } catch(Exception e) {
                return Failure(e);
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

        EvaluationResult Success(object value){ return EvaluationResult.Success(value); }
        EvaluationResult Failure(Exception e){ return EvaluationResult.Failure(e); } 

    }
}
