using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Cone
{
    public class ExpressionFormatter
    {
        public string Format(Expression expr) {
            switch (expr.NodeType) {
                case ExpressionType.ArrayLength: return FormatUnary(expr) + ".Length";                
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expr;
                    if (member.Expression == null)
                        return member.Member.DeclaringType.Name + "." + member.Member.Name;
                    if (member.Expression.NodeType == ExpressionType.Constant)
                        return member.Member.Name;
                    return Format(member.Expression) + "." + member.Member.Name;
                case ExpressionType.Call:
                    var call = (MethodCallExpression)expr;
                    int firstArgument;
                    return FormatCallTarget(call, out firstArgument) + "." + call.Method.Name + FormatArgs(call.Arguments, firstArgument);
                case ExpressionType.Quote: return FormatUnary(expr);
                case ExpressionType.Lambda:
                    var lambda = (LambdaExpression)expr;
                    return FormatArgs(lambda.Parameters) + " => " + Format(lambda.Body);
                case ExpressionType.Equal: return FormatBinary(expr, " == ");
                case ExpressionType.NotEqual: return FormatBinary(expr, " != ");
                case ExpressionType.Constant:
                    var constant = (ConstantExpression)expr;
                    if(constant.Type != typeof(Type))
                        return constant.ToString();
                    var type = (Type)constant.Value;
                    return "typeof(" + type.Name + ")";
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

        string FormatArgs(IList<ParameterExpression> args) {
            var items = new string[args.Count];
            for (int i = 0; i != items.Length; ++i)
                items[i] = Format(args[i]);
            return FormatArgs(items);
        }

        string FormatArgs(IList<Expression> args, int first) {
            var items = new string[args.Count - first];
            for (int i = 0; i != items.Length; ++i)
                items[i] = Format(args[first +  i]);
            return FormatArgs(items);
        }

        string FormatArgs(string[] value) {
            return "(" + string.Join(", ", value) + ")";
        }

        string FormatBinary(Expression expr, string op) {
            var binary = (BinaryExpression)expr;
            return Format(binary.Left) + op + Format(binary.Right);
        }

        string FormatUnary(Expression expr) {
            return Format(((UnaryExpression)expr).Operand);
        }
    }
}
