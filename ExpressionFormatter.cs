using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Cone
{
    public interface IExpressionFormatter
    {
        string Format(Expression expression);
    }

    public class ExpressionFormatter : IExpressionFormatter
    {
        const string IndexerGet = "get_Item";

        public string Format(Expression expression) {
            switch (expression.NodeType) {
                case ExpressionType.ArrayLength: return FormatUnary(expression) + ".Length";
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expression;
                    if (member.Expression == null)
                        return member.Member.DeclaringType.Name + "." + member.Member.Name;
                    if (member.Expression.NodeType == ExpressionType.Constant)
                        return member.Member.Name;
                    return Format(member.Expression) + "." + member.Member.Name;
                case ExpressionType.Call:
                    var call = (MethodCallExpression)expression;
                    int firstArgumentOffset;
                    var target = FormatCallTarget(call, out firstArgumentOffset);
                    var method = call.Method;
                    var invocation = string.Empty;
                    var parameterFormat = "({0})";
                    if(method.IsSpecialName && IndexerGet == method.Name)
                        parameterFormat = "[{0}]";
                    else if (call.Object != null && call.Object.NodeType == ExpressionType.Constant) {
                        target = string.Empty;
                        invocation = method.Name;
                    } else
                        invocation = "." + method.Name;
                    return target + invocation + FormatArgs(call.Arguments, firstArgumentOffset, parameterFormat);
                case ExpressionType.Quote: return FormatUnary(expression);
                case ExpressionType.Lambda:
                    var lambda = (LambdaExpression)expression;
                    return FormatArgs(lambda.Parameters) + " => " + Format(lambda.Body);
                case ExpressionType.Constant: return FormatConstant((ConstantExpression)expression);
                case ExpressionType.Convert:
                    var convert = (UnaryExpression)expression;
                    return string.Format("({0}){1}", FormatType(convert.Type), Format(convert.Operand));

                default:
                    var binaryOp = GetBinaryOp(expression.NodeType);
                    if (string.IsNullOrEmpty(binaryOp))
                        return expression.ToString();
                    else
                        return FormatBinary(expression, binaryOp);
            }
        }

        string FormatType(Type type) {
            switch(type.FullName) {
                case "System.Object": return "object";
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

        static string GetBinaryOp(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Add: return "+";
                case ExpressionType.Equal: return "==";
                case ExpressionType.NotEqual: return "!=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                default: return string.Empty;
            }
        }

        string FormatBinary(Expression expr, string op) {
            var binary = (BinaryExpression)expr;
            return FormatBinary(binary.Left, binary.Right, op);
        }
        string FormatBinary(Expression left, Expression right, string op) {
            if (left.NodeType == ExpressionType.Convert) {
                var convert = (UnaryExpression)left;
                left = convert.Operand;
                if (right.NodeType == ExpressionType.Constant && right.Type.Equals(typeof(int))) {
                    var newValue = Enum.ToObject(convert.Operand.Type, (int)((ConstantExpression)right).Value);
                    right = Expression.Constant(newValue);
                }
            }
            return string.Format("{0} {1} {2}", Format(left), op, Format(right));
        }

        string FormatUnary(Expression expr) {
            return Format(((UnaryExpression)expr).Operand);
        }
    }
}
