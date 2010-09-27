using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Cone
{
    public interface IFormatter<T>
    {
        string Format(T expression);
    }

    public class ExpressionFormatter : IFormatter<Expression>
    {
        const string IndexerGet = "get_Item";

        public string Format(Expression expression) {
            switch (expression.NodeType) {
                case ExpressionType.ArrayLength: return FormatUnary((UnaryExpression)expression) + ".Length";
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expression;
                    if (member.Expression == null)
                        return member.Member.DeclaringType.Name + "." + member.Member.Name;
                    if (member.Expression.NodeType == ExpressionType.Constant)
                        return member.Member.Name;
                    return Format(member.Expression) + "." + member.Member.Name;
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

        string FormatConstant(ConstantExpression constant) {
            if (constant.Type.IsEnum)
                return constant.Type.Name + "." + constant.Value.ToString();
            else if (constant.Type != typeof(Type))
                return constant.ToString();
            var type = (Type)constant.Value;
            return "typeof(" + type.Name + ")";
        }

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
