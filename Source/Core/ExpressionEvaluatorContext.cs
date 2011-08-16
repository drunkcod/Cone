using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Core
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
                case ExpressionType.Constant: return Success(body.Type, ((ConstantExpression)body).Value);
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
                return Success(null, null);
            return Evaluate(expression)
                .Maybe(result => result.IsNull ? NullSubexpression(expression, context) : result);
        }

        public EvaluationResult EvaluateAll(ICollection<Expression> expressions) {
            var result = new object[expressions.Count];
            var index = 0;
            foreach(var item in expressions) {
                var x = Evaluate(item);
                if(x.IsError)
                    return x;
                result[index++] = x.Result;
            }
            return Success(typeof(object[]), result);
        }

        EvaluationResult EvaluateLambda(Expression expression) { return EvaluateLambda((LambdaExpression)expression); }
        EvaluationResult EvaluateLambda(LambdaExpression expression) {
            if(expression == context && expression.Parameters.Count == 0)
                return Evaluate(expression.Body);
            return Success(expression.Type, expression.Compile());
        }

        EvaluationResult EvaluateArrayIndex(Expression expression) {
            var rank1 = expression as BinaryExpression;
            if(rank1 != null) 
                return EvaluateArrayIndex1(rank1);
            return Unsupported(expression);
        }

        EvaluationResult EvaluateArrayIndex1(BinaryExpression rank1) {
            var array = (Array)Evaluate(rank1.Left).Result;
            var index = (int)Evaluate(rank1.Right).Result;
            return Success(rank1.Type, array.GetValue(index));
        }

        EvaluationResult EvaluateBinary(Expression expression) { return EvaluateBinary((BinaryExpression)expression); }
        EvaluationResult EvaluateBinary(BinaryExpression binary) {
            return Evaluate(binary.Left)
                .Maybe(left => Evaluate(binary.Right)
                .Maybe(right => EvaluateBinary(binary, left.Result, right.Result)));
        }

        EvaluationResult EvaluateBinary(BinaryExpression binary, object left, object right) {
            var op = binary.Method;
            if(op != null)
                return Success(op.ReturnType, op.Invoke(null, new[]{ left, right }));
            switch(binary.NodeType) {
                case ExpressionType.Equal: return Success(typeof(bool), Object.Equals(left, right));
                case ExpressionType.NotEqual: return Success(typeof(bool), !Object.Equals(left, right));
                default: return Unsupported(binary);
            }
        }


        EvaluationResult EvaluateCall(Expression expression) { return EvaluateCall((MethodCallExpression)expression); }
        EvaluationResult EvaluateCall(MethodCallExpression expression) {
            return EvaluateAsTarget(expression.Object).Maybe(target => 
                EvaluateAll(expression.Arguments).Maybe(arguments => {
                    var method = expression.Method;
                    var input = (object[])arguments.Result;
                    return GuardedInvocation(expression, 
                        () => Success(method.ReturnType, method.Invoke(target.Result, input)), 
                        () => AssignOutParameters(expression.Arguments, input, method.GetParameters()));
                }));
        }

        void AssignOutParameters(IList<Expression> arguments, object[] results, ParameterInfo[] parameters) {
            if(results.Length == 0)
                return;
            for(int i = 0; i != parameters.Length; ++i)
                if(parameters[i].IsOut) {
                    var member = (arguments[i] as MemberExpression);
                    var field = member.Member as FieldInfo;
                    field.SetValue(Rebind(member.Expression).Evaluate(member.Expression).Result, results[i]);
                }
        }
        
        EvaluationResult EvaluateConvert(Expression expression) { return EvaluateConvert((UnaryExpression)expression); }
        EvaluationResult EvaluateConvert(UnaryExpression expression) {
            return Evaluate(expression.Operand).Maybe(source => {
                var value = source.Result;
                var convertMethod = expression.Method;
                if(convertMethod != null && convertMethod.IsStatic) {
                    return GuardedInvocation(expression, () => Success(convertMethod.ReturnType, convertMethod.Invoke(null, new[] { value })));
                }
                return Success(expression.Type, ChangeType(value, expression.Type));
            });
        }

        EvaluationResult EvaluateMemberAccess(Expression expression) { return EvaluateMemberAccess((MemberExpression)expression); }
        EvaluationResult EvaluateMemberAccess(MemberExpression expression) {
            return GuardedInvocation(expression, () =>
                EvaluateAsTarget(expression.Expression)
                    .Maybe(x => Success(expression.Type, expression.Member.GetValue(x.Result))));
        }

        EvaluationResult EvaluateNew(Expression expression) { return EvaluateNew((NewExpression)expression); }
        EvaluationResult EvaluateNew(NewExpression expression) {
            return GuardedInvocation(expression, () => {
                var args = EvaluateAll(expression.Arguments).Result as object[];
                if(expression.Constructor != null)
                    return Success(expression.Type, expression.Constructor.Invoke(args));
                return Success(expression.Type, Activator.CreateInstance(expression.Type, args));
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

        EvaluationResult EvaluateQuote(Expression expression) { return EvaluateQuote((UnaryExpression)expression); }
        EvaluationResult EvaluateQuote(UnaryExpression expression) {
            return Success(expression.Type, expression.Operand);
        }

        EvaluationResult EvaluateInvoke(Expression expression) { return EvaluateInvoke((InvocationExpression)expression); }
        EvaluationResult EvaluateInvoke(InvocationExpression expression) {
            var target = Evaluate(expression.Expression).Result as Delegate;
            return GuardedInvocation(expression, () => Success(expression.Type, target.DynamicInvoke(EvaluateAll(expression.Arguments).Result as object[])));
        }

        ExpressionEvaluatorContext Rebind(Expression context) {
            return new ExpressionEvaluatorContext(context) {
                Unsupported = Unsupported,
                NullSubexpression = NullSubexpression
            };
        }

        object ChangeType(object value, Type to) {
            return ObjectConverter.ChangeType(value, to);
        }

        EvaluationResult Success(Type type, object value){ return EvaluationResult.Success(type, value); }
        EvaluationResult Failure(Expression expression, Exception e){ return EvaluationResult.Failure(expression, e); } 
    }
}
