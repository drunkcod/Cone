using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Cone
{
    public class ExpressionFormatter
    {
        const string IndexerGet = "get_Item";

        public string Format(Expression expr) {
            switch (expr.NodeType) {
                case ExpressionType.ArrayLength: return FormatUnary(expr) + ".Length";
                case ExpressionType.Add: goto case ExpressionType.Equal;
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expr;
                    if (member.Expression == null)
                        return member.Member.DeclaringType.Name + "." + member.Member.Name;
                    if (member.Expression.NodeType == ExpressionType.Constant)
                        return member.Member.Name;
                    return Format(member.Expression) + "." + member.Member.Name;
                case ExpressionType.Call:
                    var call = (MethodCallExpression)expr;
                    int firstArgumentOffset;
                    var target = FormatCallTarget(call, out firstArgumentOffset);
                    var method = call.Method;
                    var invocation = string.Empty;
                    var parameterFormat = "({0})";
                    if(method.IsSpecialName && IndexerGet == method.Name)
                        parameterFormat = "[{0}]";
                    else 
                        invocation = "." + method.Name;
                    return target + invocation + FormatArgs(call.Arguments, firstArgumentOffset, parameterFormat);
                case ExpressionType.Quote: return FormatUnary(expr);
                case ExpressionType.Lambda:
                    var lambda = (LambdaExpression)expr;
                    return FormatArgs(lambda.Parameters) + " => " + Format(lambda.Body);
                case ExpressionType.Equal: return FormatBinary(expr, GetBinaryOp);
                case ExpressionType.NotEqual: goto case ExpressionType.Equal;
                case ExpressionType.Constant: return FormatConstant((ConstantExpression)expr);
            }
            return expr.ToString();
        }

        string FormatCallTarget(MethodCallExpression call, out int firstArgument) {
            firstArgument = 0;
            if (call.Object == null) {
                if(call.Method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false).Length == 0)
                    return call.Method.DeclaringType.Name;
                firstArgument = 1;
                return Format(call.Arguments[0]);
            }
            return Format(call.Object);
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

        public string FormatBinary(Expression expr, Func<ExpressionType, string> getOp) {
            var binary = (BinaryExpression)expr;
            return FormatBinary(binary.Left, binary.Right, getOp(binary.NodeType));
        }

        static string GetBinaryOp(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Add: return "{0} + {1}";
                case ExpressionType.Equal: return "{0} == {1}";
                case ExpressionType.NotEqual: return "{0} != {1}";
                default: return "{0} ? {1}";
            }
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
            return string.Format(op, Format(left), Format(right));
        }

        string FormatUnary(Expression expr) {
            return Format(((UnaryExpression)expr).Operand);
        }
    }
}
