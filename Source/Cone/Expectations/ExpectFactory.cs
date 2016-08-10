using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
{
	public class ExpectFactory
	{
		delegate Expect Expector(BinaryExpression body, IExpectValue left, IExpectValue right);

		static readonly Expector EqualExpector = (body, left, right) => new EqualExpect(body, left, right);
		static readonly Expector NotEqualExpector = (body, left, right) => new NotEqualExpect(body, left, right);     
		static readonly Expector BinaryExpector = (body, left, right) => new BinaryExpect(body, left, right);
		static readonly Expector LessThanExpector = (body, left, right) => new LessThanExpect(body, left, right);     
		static readonly Expector LessThanOrEqualExpector = (body, left, right) => new LessThanOrEqualExpect(body, left, right);     
		static readonly Expector GreaterThanExpector = (body, left, right) => new GreaterThanExpect(body, left, right);
		static readonly Expector GreaterThanOrEqualExpector = (body, left, right) => new GreaterThanOrEqualExpect(body, left, right);
		static readonly ExpressionEvaluator Evaluator = new ExpressionEvaluator();

		readonly MethodExpectProviderLookup methodExpects = new MethodExpectProviderLookup();

		public ExpectFactory(IEnumerable<Assembly> assembliesToScan) {
			var providers = assembliesToScan
				.SelectMany(x => x.GetExportedTypes())
				.Where(IsMethodExpectProvider)
				.Select(x => x.New() as IMethodExpectProvider);
			foreach(var provider in providers)
				foreach(var method in provider.GetSupportedMethods())
					methodExpects.Insert(method, provider);
		}

		public static bool IsMethodExpectProvider(Type type) {
			return type.IsVisible && type.IsClass && type.Implements<IMethodExpectProvider>();
		}

		public IExpect From(Expression body) {
			switch(body.NodeType) {
				case ExpressionType.Not: return new NotExpect(From(((UnaryExpression)body).Operand));
				case ExpressionType.AndAlso: return Boolean(body);
				case ExpressionType.Invoke: return Boolean(body);
				case ExpressionType.Convert:
					var conversion = (UnaryExpression)body;
					if(conversion.Type == typeof(bool))
						return Conversion(conversion);
					break;
			}

			if (SupportedExpressionType(body.NodeType))
				return Lambda(body, null);
			throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
		}

		public IExpect FromLambda(Expression body, ExpressionEvaluatorParameters parameters) {
			return Lambda(body, parameters);
		} 

		static bool SupportedExpressionType(ExpressionType nodeType) {
			switch (nodeType) {
				case ExpressionType.Call: return true;
				case ExpressionType.Constant: return true;
				case ExpressionType.Equal: return true;
				case ExpressionType.NotEqual: return true;
				case ExpressionType.GreaterThan: return true;
				case ExpressionType.GreaterThanOrEqual: return true;
				case ExpressionType.LessThan: return true;
				case ExpressionType.LessThanOrEqual: return true;
				case ExpressionType.MemberAccess: return true;
				case ExpressionType.TypeIs: return true;
			}
			return false;
		}

		IExpect Lambda(Expression body, ExpressionEvaluatorParameters parameters) {
			var binary = body as BinaryExpression;
			if (binary != null)
				return Binary(LiftEnum(binary), parameters);
			if(body.NodeType == ExpressionType.TypeIs)
				return TypeIs((TypeBinaryExpression)body);
			return Unary(body, parameters);
		}

		BinaryExpression LiftEnum(BinaryExpression source) {
			if(source.Right.NodeType == ExpressionType.Constant
			&& source.Left.NodeType == ExpressionType.Convert
			&& source.Right.Type == source.Right.Type) {
				var left = (UnaryExpression)source.Left;
				var right = (ConstantExpression)source.Right;
				if(left.Operand.Type.IsEnum) {
					return Expression.MakeBinary(source.NodeType, 
						left.Operand,
						Expression.Constant(Enum.ToObject(left.Operand.Type, right.Value)));
				}
			}
			return source;
		}

		IExpect Unary(Expression body, ExpressionEvaluatorParameters parameters) {
			if(body.NodeType == ExpressionType.Call)
				return Method((MethodCallExpression)body, parameters);
			return Boolean(body);
		}

		IExpect Method(MethodCallExpression body, ExpressionEvaluatorParameters parameters) {
			IMethodExpectProvider provider;
			var method = body.Method;
			if(TryGetExpectProvider(method, out provider)) {
				var target = Evaluator.EvaluateAsTarget(body.Object, body, parameters).Result;
				var args = body.Arguments.ConvertAll(x => EvaluateAs<object>(x, null));
				return provider.GetExpectation(body, method, target, args);
			}
			return Boolean(body);
		}

		bool TryGetExpectProvider(MethodInfo method, out IMethodExpectProvider provider) {
			return methodExpects.TryGetExpectProvider(method, out provider);
		}

		IExpect Boolean(Expression body) {
			return new BooleanExpect(body, new ExpectValue(EvaluateAs<bool>(body, null)));
		}

		IExpect Conversion(UnaryExpression conversion) {
			return new ConversionExpect(conversion, EvaluateAs<object>(conversion.Operand, null), conversion.Method);
		}

		static Expect Binary(BinaryExpression body, ExpressionEvaluatorParameters parameters) {
			var left = Evaluate(body.Left, body, parameters);
			var right = Evaluate(body.Right, body, parameters);

			if(IsStringEquals(body))
				return new StringEqualExpect(body, (string)left.Value, (string)right.Value);

			return GetExpector(body.NodeType)(body, left, right);
		}

		static bool IsStringEquals(BinaryExpression body) {
			return body.NodeType == ExpressionType.Equal
			&& body.Left.Type == typeof(string) 
			&& body.Right.Type == typeof(string);
		}

		static Expector GetExpector(ExpressionType op) {
			switch(op) {
				case ExpressionType.Equal: return EqualExpector;
				case ExpressionType.NotEqual: return NotEqualExpector;
				case ExpressionType.LessThan: return LessThanExpector;
				case ExpressionType.LessThanOrEqual: return LessThanOrEqualExpector;
				case ExpressionType.GreaterThan: return GreaterThanExpector;
				case ExpressionType.GreaterThanOrEqual: return GreaterThanOrEqualExpector;
			}
			return BinaryExpector;
		}
		
		class WrappedExpectValue : IExpectValue
		{
			readonly object value;
			readonly object rawValue;

			public WrappedExpectValue(object value, object rawValue) {
				this.value = value;
				this.rawValue = rawValue;
			}

			public object Value { get { return value;} }

			public string ToString(IFormatter<object> formatter) { return formatter.Format(rawValue); }
			public override string ToString() { return rawValue.ToString(); }
		}

		static T EvaluateAs<T>(Expression body, ExpressionEvaluatorParameters parameters) { return (T)(Evaluate(body, body, parameters).Value); }
		
		static IExpectValue Evaluate(Expression body, Expression context, ExpressionEvaluatorParameters parameters) { 
			var unwrapped = Evaluator.Unwrap(body);
			var value = Evaluator.Evaluate(body, context, parameters).Result;
			if(unwrapped == body)
				return new ExpectValue(value); 
			return new WrappedExpectValue(value, Evaluator.Evaluate(unwrapped, context).Result);
		}

		static Expect TypeIs(TypeBinaryExpression body) {
			return new TypeIsExpect(body,
				Evaluate(body.Expression, body, null), 
				body.TypeOperand);
		}
	}
}
