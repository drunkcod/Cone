using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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

		public ExpressionFormatter Rebind(Type newContext) {
			if(newContext == context)
				return this;
			return new ExpressionFormatter(newContext, constantFormatter);
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
				case ExpressionType.Conditional: return FormatConditional(expression);
				case ExpressionType.Convert: return FormatConvert(expression);
				case ExpressionType.Default: return FormatDefault(expression);
				case ExpressionType.TypeIs: return FormatTypeIs(expression);
				case ExpressionType.TypeAs: return FormatTypeAs(expression);
				case ExpressionType.Invoke: return FormatInvoke(expression);
					
				default:
					var binary = expression as BinaryExpression;
					return binary == null 
						? expression.ToString() 
						: FormatBinary(binary);
			}
		}

		string FormatArrayLength(Expression expression){ return FormatArrayLength((UnaryExpression)expression); }
		string FormatArrayLength(UnaryExpression arrayLength) {
			return FormatUnary(arrayLength) + ".Length";
		}

		string FormatCallTarget(MethodCallExpression call, out int firstArgument) {
			firstArgument = 0;
			var target = call.Object;
			if (target == null) {
				if(!IsExtensionMethod(call.Method))
					return call.Method.DeclaringType.Name;
				firstArgument = 1;
				return Format(call.Arguments[0]);
			}
			return Format(target);
		}

		static bool IsExtensionMethod(MethodInfo method) {
			return method.GetCustomAttributes(typeof(ExtensionAttribute), false).Length != 0;
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

		string FormatConditional(Expression expression) { return FormatConditional((ConditionalExpression)expression); }
		string FormatConditional(ConditionalExpression conditional) {
			return string.Format("{0} ? {1} : {2}", Format(conditional.Test), Format(conditional.IfTrue), Format(conditional.IfFalse));
		}

		string FormatConvert(Expression expression) { return FormatConvert((UnaryExpression)expression); }
		string FormatConvert(UnaryExpression conversion) {
			if(conversion.Type == typeof(object))
				return Format(conversion.Operand);
			var operandMethod = conversion.Operand as MethodCallExpression;
			if(operandMethod != null && operandMethod.Method == typeof(Delegate).GetMethod("CreateDelegate", new []{ typeof(Type), typeof(object), typeof(MethodInfo) }))
				return ((operandMethod.Arguments[2] as ConstantExpression).Value as MethodInfo).Name;
			if(operandMethod != null && operandMethod.Method == typeof(MethodInfo).GetMethod("CreateDelegate", new []{ typeof(Type), typeof(object) }))
				return ((operandMethod.Object as ConstantExpression).Value as MethodInfo).Name;
			return string.Format("({0}){1}", FormatType(conversion.Type), Format(conversion.Operand));
		}

		string FormatDefault(Expression expression) { return FormatDefault((DefaultExpression)expression); }

		string FormatDefault(DefaultExpression defalt) {
			return string.Format("default({0})", FormatType(defalt.Type));
		}

		string FormatTypeIs(Expression expression) { return FormatTypeIs((TypeBinaryExpression)expression); }
		string FormatTypeIs(TypeBinaryExpression typeIs) {
			return string.Format("{0} is {1}", Format(typeIs.Expression), FormatType(typeIs.TypeOperand));
		}

		string FormatTypeAs(Expression expression) { return FormatTypeAs((UnaryExpression)expression); }
		string FormatTypeAs(UnaryExpression typeAs) {
			return string.Format("({0} as {1})", Format(typeAs.Operand), FormatType(typeAs.Type));
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
			return FormatJoin(args.ConvertAll(Format), MethodArgumentsFormat);
		}

		string FormatArgs(IList<Expression> args, int first, string format) {
			var items = new string[args.Count - first];
			for (var i = 0; i != items.Length; ++i)
				items[i] = Format(args[first +  i]);
			return FormatJoin(items, format);
		}

		string FormatJoin(string[] value, string format) {
			return string.Format(format, value.Join(", "));
		}

		string FormatBinary(BinaryExpression binary) {
			Expression left = binary.Left, right = binary.Right;
			if (left.NodeType == ExpressionType.Convert) {
				left = (left as UnaryExpression).Operand;
			}
			if(left.NodeType == ExpressionType.Constant && right.NodeType == ExpressionType.Convert) {
				var conversion = (right as UnaryExpression).Operand;
				if(conversion.Type.IsEnum)
					return FormatBinary(Expression.MakeBinary(binary.NodeType, 
						EnumConstant(conversion.Type, (left as ConstantExpression).Value), conversion));
			}
			if(left.Type.IsEnum && right.Type == Enum.GetUnderlyingType(left.Type)) {
				return FormatBinary(Expression.MakeBinary(binary.NodeType, 
					left, UnpackEnum(left.Type, right)));
			}
			var format = string.Format(GetBinaryOp(binary.NodeType), BinaryFormat(left, 0), BinaryFormat(right, 1));
			return string.Format(format, Format(left), Format(right));
		}

		Expression UnpackEnum(Type enumType, Expression expression) {
			switch(expression.NodeType) {
				case ExpressionType.Constant: return EnumConstant(enumType, (expression as ConstantExpression).Value);
				case ExpressionType.Convert: return (expression as UnaryExpression).Operand;
				default: throw new InvalidOperationException("Unsupported expression type - " + expression.NodeType);
			}
		}

		Expression EnumConstant(Type enumType, object value) { return Expression.Constant(Enum.ToObject(enumType, value)); }

		string BinaryFormat(Expression expression, int index) {
			bool needParens = 
				expression.NodeType == ExpressionType.Conditional
				|| expression is BinaryExpression;
			return string.Format(needParens ? "({{{0}}})" : "{{{0}}}", index);
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

		string FormatNew(Expression expression){ return FormatNew((NewExpression)expression, false); }
		string FormatNew(NewExpression expression, bool canSkipArgs) {
			var type = expression.Type;
			if(!type.Has<CompilerGeneratedAttribute>()) {
				var pre = "new " + FormatType(expression.Type);
				if(canSkipArgs && expression.Arguments.Count == 0)
					return pre + " ";
				return pre + FormatArgs(expression.Arguments, 0, MethodArgumentsFormat);
			}
			var result = new StringBuilder("new {");
			var sep = " ";
			var parameters = expression.Constructor.GetParameters();
			for(var  i = 0; i != parameters.Length; ++i) {
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
			var result = new StringBuilder(FormatNew(memberInit.NewExpression, true));

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

		string FormatType(Type type) {
			return TypeFormatter.Format(type);
		}

		bool IsAnonymousOrContextMember(Expression expression) {
			if(expression == null || expression.NodeType != ExpressionType.Constant)
				return false;
			var valueType = (expression as ConstantExpression).Value.GetType();
			return valueType == context || IsCompilerGenerated(valueType);
		}

		public static bool IsCompilerGenerated(Type type) {
			if(type == null)
				return false;
			return type.Has<CompilerGeneratedAttribute>()
				|| IsCompilerGenerated(type.DeclaringType);
		}

		static string GetBinaryOp(ExpressionType nodeType) {
			switch (nodeType) {
				case ExpressionType.Add: return "{0} + {1}";
				case ExpressionType.Subtract: return "{0} - {1}";
				case ExpressionType.Multiply: return "{0} * {1}";
				case ExpressionType.Divide: return "{0} / {1}";
				case ExpressionType.Modulo: return "{0} % {1}";
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
				case ExpressionType.LeftShift: return "{0} << {1}";
				case ExpressionType.RightShift: return "{0} >> {1}";
				default: throw new NotSupportedException("Unsupported BinaryExression type " + nodeType);
			}
		}
	}
}
