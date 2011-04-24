using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    class ExpressionEvaluatorContext 
    {
        static readonly Dictionary<KeyValuePair<Type, Type>, Func<object, object>> converters = new Dictionary<KeyValuePair<Type, Type>,Func<object,object>>();

        readonly Expression context;

        public Func<Expression,EvaluationResult> Unsupported;
        public Func<Expression, Expression, EvaluationResult> NullSubexpression;

        public ExpressionEvaluatorContext(Expression context) {
            this.context = context;
        }

        public EvaluationResult Evaluate(Expression body) {
            switch(body.NodeType) {
                case ExpressionType.Lambda: return EvaluateLambda(body as LambdaExpression);
                case ExpressionType.ArrayIndex: return EvaluateArrayIndex(body);
                case ExpressionType.Call: return EvaluateCall(body as MethodCallExpression);
                case ExpressionType.Constant: return Success((body as ConstantExpression).Value);
                case ExpressionType.Convert: return EvaluateConvert(body as UnaryExpression);
                case ExpressionType.Equal: goto case ExpressionType.NotEqual;
                case ExpressionType.NotEqual: return EvaluateBinary(body as BinaryExpression);
                case ExpressionType.MemberAccess: return EvaluateMemberAccess(body as MemberExpression);
                case ExpressionType.New: return EvaluateNew(body as NewExpression);
                case ExpressionType.Quote: return EvaluateQuote(body as UnaryExpression);
                case ExpressionType.Invoke: return EvaluateInvoke(body as InvocationExpression);
                default: return Unsupported(body);
            }
        }

        public EvaluationResult EvaluateAsTarget(Expression expression) {
            if(expression == null)
                return Success(null);
            var target = Evaluate(expression);
            if(target.IsError || target.Value == null)
                return NullSubexpression(expression, context);
            return target;
        }

        EvaluationResult EvaluateLambda(LambdaExpression expression) {
            if(expression == context && expression.Parameters.Count == 0)
                return Evaluate(expression.Body);
            return Success(expression.Compile());
        }

        EvaluationResult EvaluateArrayIndex(Expression expression) {
            var rank1 = expression as BinaryExpression;
            if(rank1 != null) 
                return EvaluateArrayIndex1(rank1);
            return Unsupported(expression);
        }

        EvaluationResult EvaluateArrayIndex1(BinaryExpression rank1) {
            var array = (Array)Evaluate(rank1.Left).Value;
            var index = (int)Evaluate(rank1.Right).Value;
            return Success(array.GetValue(index));
        }

        EvaluationResult EvaluateBinary(BinaryExpression binary) {
            var left = Evaluate(binary.Left);
            if(left.IsError)
                return left;

            var right = Evaluate(binary.Right);
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
                case ExpressionType.NotEqual: return Success(!Object.Equals(parameters[0], parameters[1]));
                default: return Unsupported(binary);
            }
        }

        EvaluationResult EvaluateCall(MethodCallExpression expression) {
            var target = EvaluateAsTarget(expression.Object);
            if(target.IsError)
                return target;
            var input = EvaluateAll(expression.Arguments).Value as object[];
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
                    var memberContext = Rebind(member.Expression);
                    field.SetValue(memberContext.Evaluate(member.Expression).Value, results[i]);
                }
        }
        
        EvaluationResult EvaluateConvert(UnaryExpression expression) {
            var source = Evaluate(expression.Operand).Value;
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

        EvaluationResult EvaluateMemberAccess(MemberExpression expression) {
            try {
                var target = EvaluateAsTarget(expression.Expression);
                if(target.IsError)
                    return target;
                return Success(GetValue(target.Value, expression.Member));
            } catch(TargetInvocationException e) {
                return Failure(e.InnerException);
            }
        }

        EvaluationResult EvaluateNew(NewExpression expression) {
            try {
                var args = EvaluateAll(expression.Arguments).Value as object[];
                if(expression.Constructor != null)
                    return Success(expression.Constructor.Invoke(args));
                return Success(Activator.CreateInstance(expression.Type, args));
            } catch(TargetInvocationException e) {
                return Failure(e.InnerException);
            }
        }

        EvaluationResult EvaluateAll(ICollection<Expression> expressions) {
            var result = new object[expressions.Count];
            var index = 0;
            foreach(var item in expressions) {
                result[index++] = Evaluate(item).Value;
            }
            return Success(result);
        }

        EvaluationResult EvaluateQuote(UnaryExpression expression) {
            return Success(expression.Operand);
        }

        EvaluationResult EvaluateInvoke(InvocationExpression expression) {
            var target = Evaluate(expression.Expression).Value as Delegate;
            try {
                var args = EvaluateAll(expression.Arguments).Value as object[];
                return Success(target.DynamicInvoke(args));
            } catch(TargetInvocationException e) {
                return Failure(e.InnerException);
            }
        }

        ExpressionEvaluatorContext Rebind(Expression context) {
            return new ExpressionEvaluatorContext(context) {
                Unsupported = Unsupported,
                NullSubexpression = NullSubexpression
            };
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

        EvaluationResult Success(object value){ return EvaluationResult.Success(value); }
        EvaluationResult Failure(Exception e){ return EvaluationResult.Failure(e); } 
    }
}
