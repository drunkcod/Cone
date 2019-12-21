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
		readonly ExpressionEvaluator evaluator;
		readonly MethodExpectProviderLookup methodExpects = new MethodExpectProviderLookup();

		public bool IncludeGuide = true;

		public ExpectFactory(ExpressionEvaluator evaluator, IEnumerable<Assembly> assembliesToScan) {
			this.evaluator = evaluator;
			var providers = assembliesToScan
				.SelectMany(AssemblyMethods.GetExportedTypes)
				.Where(IsMethodExpectProvider)
				.Select(Core.TypeExtensions.New<IMethodExpectProvider>);
			foreach(var provider in providers)
			foreach(var method in provider.GetSupportedMethods())
				methodExpects.Insert(method, provider);
		}

		public static bool IsMethodExpectProvider(Type type) =>
			type.IsVisible && type.IsClass && type.Implements<IMethodExpectProvider>();

		public IExpect From(Expression body) {
			switch(body.NodeType) {
				case ExpressionType.Not: return new NotExpect(From(((UnaryExpression)body).Operand));
				case ExpressionType.AndAlso: return Boolean(body, ExpressionEvaluatorParameters.Empty);
				case ExpressionType.Invoke: return Boolean(body, ExpressionEvaluatorParameters.Empty);
				case ExpressionType.Convert:
					var conversion = (UnaryExpression)body;
					if(conversion.Type == typeof(bool))
						return Conversion(conversion);
					break;
			}

			if (SupportedExpressionType(body.NodeType))
				return Lambda(body, ExpressionEvaluatorParameters.Empty);
			throw new NotSupportedException(string.Format("Can't verify Expression of type {0}", body.NodeType));
		}

		public IExpect FromLambda(Expression body, ExpressionEvaluatorParameters parameters) => 
			Lambda(body, parameters);

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
				return TypeIs((TypeBinaryExpression)body, parameters);
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
			return Boolean(body, parameters);
		}

		IExpect Method(MethodCallExpression body, ExpressionEvaluatorParameters parameters) {
			IMethodExpectProvider provider;
			var method = body.Method;
			if(TryGetExpectProvider(method, out provider)) {
				var target = evaluator.EvaluateAsTarget(body.Object, body, parameters);
				if(target.IsError)
					return new InvocationTargetExpectFailure(body.Object, target.Exception.Message);
				var args = body.Arguments.ConvertAll(x => EvaluateAs<object>(x, ExpressionEvaluatorParameters.Empty));
				return provider.GetExpectation(body, method, target.Result, args);
			}
			return Boolean(body, parameters);
		}

		class InvocationTargetExpectFailure : IExpect
		{
			readonly Expression body;
			readonly string error;

			public InvocationTargetExpectFailure(Expression body, string error) {
				this.body = body;
				this.error = error;
			}

			public CheckResult Check() => 
				new CheckResult(false, Maybe<object>.None, Maybe<object>.None);

			public string FormatExpression(IFormatter<Expression> formatter) =>
				formatter.Format(body);

			public ConeMessage FormatMessage(IFormatter<object> formatter) =>
				ConeMessage.Combine(new[]{ new ConeMessageElement("Raised: ", "info") }, ConeMessage.Parse(error));
		}

		bool TryGetExpectProvider(MethodInfo method, out IMethodExpectProvider provider) =>
			methodExpects.TryGetExpectProvider(method, out provider);

		IExpect Boolean(Expression body, ExpressionEvaluatorParameters parameters) =>
			new BooleanExpect(body, new ExpectValue(EvaluateAs<bool>(body, parameters)));

		IExpect Conversion(UnaryExpression conversion) =>
			new ConversionExpect(conversion, EvaluateAs<object>(conversion.Operand, ExpressionEvaluatorParameters.Empty), conversion.Method);

		Expect Binary(BinaryExpression body, ExpressionEvaluatorParameters parameters) {
			var left = Evaluate(body.Left, body, parameters);
			var right = Evaluate(body.Right, body, parameters);

			if(IsStringEquals(body))
				return new StringEqualExpect(body, (string)left.Value, (string)right.Value) { IncludeGuide = IncludeGuide };
			if(body.Left.NodeType == ExpressionType.Convert && body.Left.Type == typeof(int)
				&& (body.Left as UnaryExpression).Operand.Type == typeof(char)
			)
				right = new WrappedExpectValue(right.Value, Convert.ToChar(right.Value));

			return MakeExpect(body, left, right);
		}

		static bool IsStringEquals(BinaryExpression body) {
			return body.NodeType == ExpressionType.Equal
			&& body.Left.Type == typeof(string) 
			&& body.Right.Type == typeof(string);
		}

		static Expect MakeExpect(BinaryExpression body, IExpectValue left, IExpectValue right) {
			switch(body.NodeType) {
				case ExpressionType.Equal: return new EqualExpect(body, left, right);
				case ExpressionType.NotEqual: return new NotEqualExpect(body, left, right);
				case ExpressionType.LessThan: return new LessThanExpect(body, left, right);
				case ExpressionType.LessThanOrEqual: return new LessThanOrEqualExpect(body, left, right);
				case ExpressionType.GreaterThan: return new GreaterThanExpect(body, left, right);
				case ExpressionType.GreaterThanOrEqual: return new GreaterThanOrEqualExpect(body, left, right);
			}
			return new BinaryExpect(body, left, right);
		}
		
		class WrappedExpectValue : IExpectValue
		{
			readonly object value;
			readonly object rawValue;

			public WrappedExpectValue(object value, object rawValue) {
				this.value = value;
				this.rawValue = rawValue;
			}

			public object Value => value;

			public string ToString(IFormatter<object> formatter) => formatter.Format(rawValue);
			public override string ToString() => rawValue.ToString();
		}

		T EvaluateAs<T>(Expression body, ExpressionEvaluatorParameters parameters) => (T)(Evaluate(body, body, parameters).Value);
		
		IExpectValue Evaluate(Expression body, Expression context, ExpressionEvaluatorParameters parameters) { 
			Expression unwrapped;
			var value = evaluator.Evaluate(body, context, parameters).Result;
			if(evaluator.TryUnwrap(body, out unwrapped))
				return new WrappedExpectValue(value, evaluator.Evaluate(unwrapped, context, parameters).Result);
			return new ExpectValue(value); 
		}

		Expect TypeIs(TypeBinaryExpression body, ExpressionEvaluatorParameters parameters) =>
			new TypeIsExpect(body,
				body.Expression.Type,
				Evaluate(body.Expression, body, parameters), 
				body.TypeOperand);
	}
}
