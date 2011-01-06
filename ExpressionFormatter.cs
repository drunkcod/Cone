using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace Cone
{   
    public class ExpressionFormatter : IFormatter<Expression>
    {
        const string IndexerGet = "get_Item";

        readonly Type context;
        readonly IFormatter<object> constantFormatter = new ParameterFormatter();

        public ExpressionFormatter(Type context) {
            this.context = context;
        }

        public string Format(Expression expression) {
            switch (expression.NodeType) {
                case ExpressionType.ArrayLength: return FormatUnary((UnaryExpression)expression) + ".Length";
                case ExpressionType.NewArrayInit: return FormatNewArray((NewArrayExpression)expression);
                case ExpressionType.New: return FormatNew((NewExpression)expression);
                case ExpressionType.MemberAccess:
                    var context = (Type)null;
                    var memberAccess = (MemberExpression)expression;
                    if (memberAccess.Expression == null)
                        return memberAccess.Member.DeclaringType.Name + "." + memberAccess.Member.Name;
                    if (IsAnonymousOrContextMember(memberAccess))
                        return memberAccess.Member.Name;
                    return Format(memberAccess.Expression) + "." + memberAccess.Member.Name;
                case ExpressionType.Call: return FormatCall((MethodCallExpression)expression);
                case ExpressionType.Quote: return FormatUnary((UnaryExpression)expression);
                case ExpressionType.Lambda: return FormatLambda((LambdaExpression)expression);
                case ExpressionType.Constant: return FormatConstant((ConstantExpression)expression);
                case ExpressionType.Convert:
                    var convert = (UnaryExpression)expression;
                    return string.Format("({0}){1}", FormatType(convert.Type), Format(convert.Operand));
                case ExpressionType.TypeIs:
                    var typeIs = (TypeBinaryExpression)expression;
                    return string.Format("{0} is {1}", Format(typeIs.Expression), FormatType(typeIs.TypeOperand));
                default:
                    var binary = expression as BinaryExpression;
                    if (binary == null)
                        return expression.ToString();
                    else
                        return FormatBinary(binary);
            }
        }

        bool IsAnonymousOrContextMember(MemberExpression memberAccess) {
            var expression = memberAccess.Expression;
            if(expression.NodeType != ExpressionType.Constant)
                return false;
            var valueType = (expression as ConstantExpression).Value.GetType();
            return valueType == context || valueType.Has<CompilerGeneratedAttribute>();
        }

        string FormatCall(MethodCallExpression call) {
            int firstArgumentOffset;
            var target = FormatCallTarget(call, out firstArgumentOffset);
            var method = call.Method;
            var invocation = string.Empty;
            var parameterFormat = "({0})";
            if (method.IsSpecialName && IndexerGet == method.Name)
                parameterFormat = "[{0}]";
            else if (call.Object != null && call.Object.NodeType == ExpressionType.Constant) {
                target = string.Empty;
                invocation = method.Name;
            } else
                invocation = "." + method.Name;
            return target + invocation + FormatArgs(call.Arguments, firstArgumentOffset, parameterFormat);
        }

        string FormatType(Type type) {
            switch(type.FullName) {
                case "System.Object": return "object";
                case "System.String": return "string";
                case "System.Boolean": return "bool";
                case "System.Int32": return "int";
                default: return type.Name;
            }
        }

        string FormatCallTarget(MethodCallExpression call, out int firstArgument) {
            firstArgument = 0;
            var target = call.Object;
            if (target == null) {
                if(call.Method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false).Length == 0)
                    return call.Method.DeclaringType.Name;
                firstArgument = 1;
                return Format(call.Arguments[0]);
            }
            return Format(target);
        }

        string FormatConstant(ConstantExpression constant) { return constantFormatter.Format(constant.Value); }

        string FormatLambda(LambdaExpression lambda) {
            var parameters = lambda.Parameters;
            return string.Format("{0} => {1}",
                    parameters.Count == 1 ?
                        Format(parameters[0]) :
                        FormatArgs(parameters),
                    Format(lambda.Body));
        }

        string FormatArgs(IList<ParameterExpression> args) {
            var items = new string[args.Count];
            for (int i = 0; i != items.Length; ++i)
                items[i] = Format(args[i]);
            return FormatArgs(items, "({0})");
        }

        string FormatArgs(IList<Expression> args, int first, string format) {
            var items = new string[args.Count - first];
            for (int i = 0; i != items.Length; ++i)
                items[i] = Format(args[first +  i]);
            return FormatArgs(items, format);
        }

        string FormatArgs(string[] value, string format) {
            return string.Format(format, string.Join(", ", value));
        }

        string FormatBinary(BinaryExpression binary) {
            Expression left = binary.Left, right = binary.Right;
            if (left.NodeType == ExpressionType.Convert) {
                var convert = (UnaryExpression)left;
                left = convert.Operand;
                if (right.NodeType == ExpressionType.Constant && right.Type.Equals(typeof(int))) {
                    var newValue = Enum.ToObject(convert.Operand.Type, (int)((ConstantExpression)right).Value);
                    right = Expression.Constant(newValue);
                }
            }
            return string.Format(GetBinaryOp(binary.NodeType), Format(left), Format(right));
        }

        string FormatUnary(UnaryExpression expr) {
            return Format(expr.Operand);
        }

        string FormatNewArray(NewArrayExpression newArray) {
            var arrayFormatter = new ArrayExpressionStringBuilder<Expression>();
            return arrayFormatter.Format(newArray.Expressions, this);
        }

        string FormatNew(NewExpression newExpression) {
            return "new " + newExpression.Type.Name + FormatArgs(newExpression.Arguments, 0, "({0})");
            return newExpression.ToString();
        }
        
        static string GetBinaryOp(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Add: return "{0} + {1}";
                case ExpressionType.Equal: return "{0} == {1}";
                case ExpressionType.NotEqual: return "{0} != {1}";
                case ExpressionType.GreaterThan: return "{0} > {1}";
                case ExpressionType.GreaterThanOrEqual: return "{0} >= {1}";
                case ExpressionType.LessThan: return "{0} < {1}";
                case ExpressionType.LessThanOrEqual: return "{0} <= {1}";
                case ExpressionType.ArrayIndex: return "{0}[{1}]";
                default: throw new NotSupportedException("Unsupported BinaryExression type " + nodeType);
            }
        }
    }
}
