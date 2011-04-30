using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone
{
    class ExpressionEvaluatorContext 
    {
        readonly Expression context;

        public Func<Expression,EvaluationResult> Unsupported;
        public Func<Expression, Expression, EvaluationResult> NullSubexpression;

        public ExpressionEvaluatorContext(Expression context) {
            this.context = context;
        }

        public EvaluationResult Evaluate(Expression body) {
            switch(body.NodeType) {
                case ExpressionType.Lambda: return EvaluateLambda(body);
                case ExpressionType.ArrayIndex: return EvaluateArrayIndex(body);
                case ExpressionType.Call: return EvaluateCall(body);
                case ExpressionType.Constant: return Success(((ConstantExpression)body).Value);
                case ExpressionType.Convert: return EvaluateConvert(body);
                case ExpressionType.Equal: goto case ExpressionType.NotEqual;
                case ExpressionType.NotEqual: return EvaluateBinary(body);
                case ExpressionType.MemberAccess: return EvaluateMemberAccess(body);
                case ExpressionType.New: return EvaluateNew(body);
                case ExpressionType.Quote: return EvaluateQuote(body);
                case ExpressionType.Invoke: return EvaluateInvoke(body);
                default: return Unsupported(body);
            }
        }

        public EvaluationResult EvaluateAsTarget(Expression expression) {
            if(expression == null)
                return Success(null);
            return Evaluate(expression)
                .Maybe(result => result.Value == null ? NullSubexpression(expression, context) : result);
        }

        EvaluationResult EvaluateLambda(Expression expression) { return EvaluateLambda((LambdaExpression)expression); }
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

        EvaluationResult EvaluateBinary(Expression expression) { return EvaluateBinary((BinaryExpression)expression); }
        EvaluationResult EvaluateBinary(BinaryExpression binary) {
            return Evaluate(binary.Left).Maybe(
                left => Evaluate(binary.Right).Maybe(right => {
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
                }));
        }

        EvaluationResult EvaluateCall(Expression expression) { return EvaluateCall((MethodCallExpression)expression); }
        EvaluationResult EvaluateCall(MethodCallExpression expression) {
            return EvaluateAsTarget(expression.Object).Maybe(target => {
                var input = EvaluateAll(expression.Arguments).Value as object[];
                var method = expression.Method;

                return GuardedInvocation(expression, 
                    () => Success(method.Invoke(target.Value, input)), 
                    () => AssignOutParameters(expression.Arguments, input, method.GetParameters()));
            });
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
        
        EvaluationResult EvaluateConvert(Expression expression) { return EvaluateConvert((UnaryExpression)expression); }
        EvaluationResult EvaluateConvert(UnaryExpression expression) {
            var source = Evaluate(expression.Operand).Value;
            var convertMethod = expression.Method;
            if(convertMethod != null && convertMethod.IsStatic) {
                return GuardedInvocation(expression, () => Success(convertMethod.Invoke(null, new[] { source })));
            }
            return Success(ChangeType(source, expression.Type));
        }

        object ChangeType(object value, Type to) {
            return ObjectConverter.ChangeType(value, to);
        }

        EvaluationResult EvaluateMemberAccess(Expression expression) { return EvaluateMemberAccess((MemberExpression)expression); }
        EvaluationResult EvaluateMemberAccess(MemberExpression expression) {
            return GuardedInvocation(expression, () =>
                EvaluateAsTarget(expression.Expression)
                    .Maybe(x => Success(GetValue(x.Value, expression.Member))));
        }

        EvaluationResult EvaluateNew(Expression expression) { return EvaluateNew((NewExpression)expression); }
        EvaluationResult EvaluateNew(NewExpression expression) {
            return GuardedInvocation(expression, () => {
                var args = EvaluateAll(expression.Arguments).Value as object[];
                if(expression.Constructor != null)
                    return Success(expression.Constructor.Invoke(args));
                return Success(Activator.CreateInstance(expression.Type, args));
            });
        }

        EvaluationResult GuardedInvocation(Expression expression, Func<EvaluationResult> action) { return GuardedInvocation(expression, action, () => {}); }
        EvaluationResult GuardedInvocation(Expression expression, Func<EvaluationResult> action, Action @finally) {
            try {
                return action();
            } catch(TargetInvocationException e) {
                return Failure(expression, e.InnerException);
            } finally { @finally(); }
        }

        EvaluationResult EvaluateAll(ICollection<Expression> expressions) {
            var result = new object[expressions.Count];
            var index = 0;
            foreach(var item in expressions) {
                result[index++] = Evaluate(item).Value;
            }
            return Success(result);
        }

        EvaluationResult EvaluateQuote(Expression expression) { return EvaluateQuote((UnaryExpression)expression); }
        EvaluationResult EvaluateQuote(UnaryExpression expression) {
            return Success(expression.Operand);
        }

        EvaluationResult EvaluateInvoke(Expression expression) { return EvaluateInvoke((InvocationExpression)expression); }
        EvaluationResult EvaluateInvoke(InvocationExpression expression) {
            var target = Evaluate(expression.Expression).Value as Delegate;
            return GuardedInvocation(expression, () => Success(target.DynamicInvoke(EvaluateAll(expression.Arguments).Value as object[])));
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

        EvaluationResult Success(object value){ return EvaluationResult.Success(value); }
        EvaluationResult Failure(Expression expression, Exception e){ return EvaluationResult.Failure(expression, e); } 
    }
}
