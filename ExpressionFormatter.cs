using System.Linq.Expressions;

namespace Cone
{
    class ExpressionFormatter
    {
        public string Format(Expression expr) {
            switch (expr.NodeType) {
                case ExpressionType.ArrayLength:
                    var unary = (UnaryExpression)expr;
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
                    var args = new string[call.Arguments.Count];
                    for (int i = 0; i != args.Length; ++i)
                        args[i] = Format(call.Arguments[i]);
                    return FormatCallTarget(call) + "." + call.Method.Name + "(" + string.Join(", ", args) + ")";
            }
            return expr.ToString();
        }

        string FormatCallTarget(MethodCallExpression call) {
            if (call.Object == null)
                return call.Method.DeclaringType.Name;
            return Format(call.Object);
        }
    }
}
