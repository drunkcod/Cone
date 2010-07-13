using System.Linq.Expressions;
using System.Collections.Generic;

namespace Cone
{
    class ExpressionFormatter
    {
        public string Format(Expression expr) {
            UnaryExpression unary;
            switch (expr.NodeType) {
                case ExpressionType.ArrayLength:
                    unary = (UnaryExpression)expr;
                    return Format(unary.Operand) + ".Length";
                case ExpressionType.MemberAccess:
                    var member = (MemberExpression)expr;
                    if (member.Expression == null)
                        return member.Member.DeclaringType.Name + "." + member.Member.Name;
                    if (member.Expression.NodeType == ExpressionType.Constant)
                        return member.Member.Name;
                    return Format(member.Expression) + "." + member.Member.Name;
                case ExpressionType.Call:
                    var call = (MethodCallExpression)expr;
                    return FormatCallTarget(call) + "." + call.Method.Name + FormatArgs(call.Arguments); 
                case ExpressionType.Quote:
                    unary = (UnaryExpression)expr;
                    return Format(unary.Operand);
                case ExpressionType.Lambda:
                    var lambda = (LambdaExpression)expr;
                    return FormatArgs(lambda.Parameters) + " => " + Format(lambda.Body);
            }
            return expr.ToString();
        }

        string FormatCallTarget(MethodCallExpression call) {
            if (call.Object == null)
                return call.Method.DeclaringType.Name;
            return Format(call.Object);
        }

        string FormatArgs(IList<ParameterExpression> args) {
            var items = new string[args.Count];
            for (int i = 0; i != items.Length; ++i)
                items[i] = Format(args[i]);
            return FormatArgs(items);
        }

        string FormatArgs(IList<Expression> args) {
            var items = new string[args.Count];
            for (int i = 0; i != items.Length; ++i)
                items[i] = Format(args[i]);
            return FormatArgs(items);
        }

        string FormatArgs(string[] value) {
            return "(" + string.Join(", ", value) + ")";
        }
    }
}
