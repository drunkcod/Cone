using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cone.Core
{   
    public class ExpressionFormatter : IFormatter<Expression>
    {
        const string IndexerGet = "get_Item";
        const string MethodArgumentsFormat = "({0})";

        readonly Type context;
        readonly IFormatter<object> constantFormatter;

        public ExpressionFormatter(Type context, IFormatter<object> constantFormatter) {
            this.context = context;
            this.constantFormatter = constantFormatter;
        }

        public ExpressionFormatter(Type context): this(context, new ParameterFormatter()) { }

        public ExpressionFormatter Rebind(Type context) {
            if(context == this.context)
                return this;
            return new ExpressionFormatter(context, constantFormatter);
        }

        public string Format(Expression expression) {
            switch (expression.NodeType) {
                case ExpressionType.ArrayLength: return FormatArrayLength(expression);
                case ExpressionType.NewArrayInit: return FormatNewArray(expression);
                case ExpressionType.New: return FormatNew(expression);
                case ExpressionType.Not: return FormatNot(expression);
                case ExpressionType.MemberAccess: return FormatMemberAccess(expression);
                case ExpressionType.MemberInit: return FormatMemberInit(expression);
                case ExpressionType.Quote: return FormatUnary(expression);
                case ExpressionType.Lambda: return FormatLambda(expression);
                case ExpressionType.Call: return FormatCall(expression);
                case ExpressionType.Constant: return FormatConstant(expression);
                case ExpressionType.Convert: return FormatConvert(expression);
                case ExpressionType.TypeIs: return FormatTypeIs(expression);
                case ExpressionType.Invoke: return FormatInvoke(expression);
                    
                default:
                    var binary = expression as BinaryExpression;
                    if (binary == null)
                        return expression.ToString();
                    else
                        return FormatBinary(binary);
            }
        }

        string FormatArrayLength(Expression expression){ return FormatArrayLength((UnaryExpression)expression); }
        string FormatArrayLength(UnaryExpression arrayLength) {
            return FormatUnary(arrayLength) + ".Length";
        }

        string FormatType(Type type) {
            switch(type.FullName) {
                case "System.Object": return "object";
                case "System.String": return "string";
                case "System.Boolean": return "bool";
                case "System.Int32": return "int";
                default: 
                    if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return string.Format("{0}?", FormatType(type.GetGenericArguments()[0]));
                    return type.Name;
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

        string FormatCall(Expression expression) { return FormatCall((MethodCallExpression)expression); }
        string FormatCall(MethodCallExpression call) {
            int firstArgumentOffset;
            var target = FormatCallTarget(call, out firstArgumentOffset);
            var method = call.Method;
            var invocation = string.Empty;
            var parameterFormat = MethodArgumentsFormat;
            if (method.IsSpecialName && IndexerGet == method.Name)
                parameterFormat = "[{0}]";
            else if (IsAnonymousOrContextMember(call.Object)) {
                target = string.Empty;
                invocation = method.Name;
            } else
                invocation = "." + method.Name;
            return target + invocation + FormatArgs(call.Arguments, firstArgumentOffset, parameterFormat);
        }

        string FormatConstant(Expression expression) { return FormatConstant((ConstantExpression)expression); }
        string FormatConstant(ConstantExpression constant) { return constantFormatter.Format(constant.Value); }

        string FormatConvert(Expression expression) { return FormatConvert((UnaryExpression)expression); }
        string FormatConvert(UnaryExpression conversion) {
            if(conversion.Type == typeof(object))
                return Format(conversion.Operand);
            return string.Format("({0}){1}", FormatType(conversion.Type), Format(conversion.Operand));
        }

        string FormatTypeIs(Expression expression) { return FormatTypeIs((TypeBinaryExpression)expression); }
        string FormatTypeIs(TypeBinaryExpression typeIs) {
            return string.Format("{0} is {1}", Format(typeIs.Expression), FormatType(typeIs.TypeOperand));
        }

        string FormatLambda(Expression expression) { return FormatLambda((LambdaExpression)expression); }
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
            return FormatJoin(items, MethodArgumentsFormat);
        }

        string FormatArgs(IList<Expression> args, int first, string format) {
            var items = new string[args.Count - first];
            for (int i = 0; i != items.Length; ++i)
                items[i] = Format(args[first +  i]);
            return FormatJoin(items, format);
        }

        string FormatJoin(string[] value, string format) {
            return string.Format(format, string.Join(", ", value));
        }

        string FormatBinary(BinaryExpression binary) {
            Expression left = binary.Left, right = binary.Right;
            if (left.NodeType == ExpressionType.Convert) {
                left = (left as UnaryExpression).Operand;
            }
            if(left.NodeType == ExpressionType.Constant && right.NodeType == ExpressionType.Convert && left.Type == typeof(int)) {
                var leftConstant = left as ConstantExpression;
                var conversion = (right as UnaryExpression).Operand;
                if(conversion.Type.IsEnum)
                    return FormatBinary(Expression.MakeBinary(binary.NodeType, 
                        Expression.Constant(Enum.ToObject(conversion.Type, (int)leftConstant.Value)), conversion));
            }
            var format = string.Format(GetBinaryOp(binary.NodeType), BinaryFormat(left, 0), BinaryFormat(right, 1));
            return string.Format(format, Format(left), Format(right));
        }

        string BinaryFormat(Expression expression, int index) {
            return string.Format(expression is BinaryExpression ? "({{{0}}})" : "{{{0}}}", index);
        }

        string FormatUnary(Expression expression) { return FormatUnary((UnaryExpression)expression); }
        string FormatUnary(UnaryExpression expression) {
            return Format(expression.Operand);
        }

        string FormatNewArray(Expression expression) { return FormatNewArray((NewArrayExpression)expression); }
        string FormatNewArray(NewArrayExpression newArray) {
            var arrayFormatter = new ArrayExpressionStringBuilder<Expression>();
            return arrayFormatter.Format(newArray.Expressions, this);
        }

        string FormatNew(Expression expression){ return FormatNew((NewExpression)expression); }
        string FormatNew(NewExpression expression) {
            var type = expression.Type;
            if(!type.Has<CompilerGeneratedAttribute>())
                return "new " + FormatType(expression.Type) + FormatArgs(expression.Arguments, 0, MethodArgumentsFormat);
            var result = new StringBuilder("new {");
            var sep = " ";
            var parameters = expression.Constructor.GetParameters();
            for(int i = 0; i != parameters.Length; ++i) {
                result.AppendFormat("{0}{1} = {2}", sep, parameters[i].Name, Format(expression.Arguments[i]));
                sep = ", ";
            }
            return result.Append(" }").ToString();
        }

        string FormatNot(Expression expression){ return FormatNot((UnaryExpression)expression); }
        string FormatNot(UnaryExpression expression) {
            return "!" + Format(expression.Operand);
        }

        string FormatMemberAccess(Expression expression){ return FormatMemberAccess((MemberExpression)expression); }
        string FormatMemberAccess(MemberExpression memberAccess) {
            if (memberAccess.Expression == null)
                return memberAccess.Member.DeclaringType.Name + "." + memberAccess.Member.Name;
            if (IsAnonymousOrContextMember(memberAccess.Expression))
                return memberAccess.Member.Name;
            return Format(memberAccess.Expression) + "." + memberAccess.Member.Name;
        }
        
        string FormatMemberInit(Expression expression){ return FormatMemberInit((MemberInitExpression)expression); }
        string FormatMemberInit(MemberInitExpression memberInit) {
            var result = new StringBuilder(FormatNew(memberInit.NewExpression));

            result.Append("{ ");
            var format = "{0}";
            foreach(var item in memberInit.Bindings) {
                result.AppendFormat(format, FormatMemberBinding(item));
                format = ", {0}";
            }
            result.Append(" }");
            return result.ToString();
        }

        string FormatMemberBinding(MemberBinding binding) {
            switch(binding.BindingType) {
                case MemberBindingType.Assignment:
                    var assignment = (MemberAssignment)binding;
                    return string.Format("{0} = {1}", assignment.Member.Name, Format(assignment.Expression));
                default: throw new NotSupportedException(String.Format("Unsupported MemberBindingType '{0}'", binding.BindingType));
            }
        }

        string FormatInvoke(Expression expression) { return FormatInvoke((InvocationExpression)expression); }
        string FormatInvoke(InvocationExpression invocation) {
            return Format(invocation.Expression) + FormatArgs(invocation.Arguments, 0, MethodArgumentsFormat);
        }

        bool IsAnonymousOrContextMember(Expression expression) {
            if(expression == null || expression.NodeType != ExpressionType.Constant)
                return false;
            var valueType = (expression as ConstantExpression).Value.GetType();
            return valueType == context || valueType.Has<CompilerGeneratedAttribute>();
        }

        static string GetBinaryOp(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Add: return "{0} + {1}";
                case ExpressionType.Subtract: return "{0} - {1}";
                case ExpressionType.Multiply: return "{0} * {1}";
                case ExpressionType.Divide: return "{0} / {1}";
                case ExpressionType.Equal: return "{0} == {1}";
                case ExpressionType.NotEqual: return "{0} != {1}";
                case ExpressionType.GreaterThan: return "{0} > {1}";
                case ExpressionType.GreaterThanOrEqual: return "{0} >= {1}";
                case ExpressionType.LessThan: return "{0} < {1}";
                case ExpressionType.LessThanOrEqual: return "{0} <= {1}";
                case ExpressionType.ArrayIndex: return "{0}[{1}]";
                case ExpressionType.AndAlso: return "{0} && {1}";
                case ExpressionType.OrElse: return "{0} || {1}";
                case ExpressionType.ExclusiveOr: return "{0} ^ {1}";
                default: throw new NotSupportedException("Unsupported BinaryExression type " + nodeType);
            }
        }
    }
}
