using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cone;
using Cone.Core;
using Cone.Expectations;

namespace CheckThat
{
	/*
	 * This file contains more repitition than any sane man would normally agree with.
	 * The not so good reason for it is to keep stack frames cleaner for test runners that simply spew
	 * out the complete frame without attempting to prettify it.
	 */
	public static class Check
	{
		static Assembly ExtensionsAssembly => typeof(IMethodExpectProvider).Assembly;
		static ExpectFactory expect;

		static readonly ExpressionEvaluator Evaluator = new ExpressionEvaluator();
		static ExpectFactory Expect => expect ?? (expect = new ExpectFactory(Evaluator, GetPluginAssemblies()));
		static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();
		static readonly ExpressionFormatter ExpressionFormatter = new ExpressionFormatter(typeof(Check), ParameterFormatter);

		public static Func<IEnumerable<Assembly>> GetPluginAssemblies = GetDefaultPluginAssemblies;

		public static bool IncludeGuide 
		{
			get { return Expect.IncludeGuide; } 
			set { Expect.IncludeGuide = value; }
		}

		static IEnumerable<Assembly> GetDefaultPluginAssemblies() =>
            AppDomain.CurrentDomain.GetAssemblies().Where(ReferencesExtensionPoints);

		static bool ReferencesExtensionPoints(Assembly assembly) => 
			assembly.GetReferencedAssemblies().Any(a => a.FullName == ExtensionsAssembly.FullName);

		internal static void Initialize() => That(() => Expect != null);
		
		internal static Exception MakeFail(FailedExpectation fail, Exception innerException) =>
			new CheckFailed(string.Empty, fail, innerException);

		internal static Exception MakeFail(FailedExpectation[] fails, Exception innerException) =>
			new CheckFailed(string.Empty, fails, innerException);

		public static object That(Expression<Func<bool>> expr) {
			var result = CheckExpect(expr.Body, ExpressionEvaluatorParameters.Empty);
			if (result.IsSuccess)
				return result.Value;
			throw MakeFail(result.Error, null);
		}

		public static void That(Expression<Func<bool>> expr, params Expression<Func<bool>>[] extras) {
			var failed = Eval(new []{ expr }.Concat(extras));
			if(failed != null)
				throw failed;
		}

		public static void That(IEnumerable<Expression<Func<bool>>> exprs) {
			var failed = Eval(exprs);
			if(failed != null)
				throw failed;
		}

		internal static Exception Eval(IEnumerable<Expression<Func<bool>>> exprs) {
			var failed = GetFailed(exprs, x => CheckExpect(x.Body, ExpressionEvaluatorParameters.Empty));
			return failed.Length > 0 
			? MakeFail(failed, null) 
			: null;
		}

		static FailedExpectation[] GetFailed<T>(IEnumerable<T> xs, Func<T,EvalResult> check) =>
			xs.Select(check).Where(x => !x.IsSuccess).Select(x => x.Error).ToArray();

		public static TException Exception<TException>(Expression<Action> expr) where TException : Exception => Check<TException>.When(expr);

		internal static IExpect ToExpect(Expression body, ExpressionEvaluatorParameters parameters) {
			try {
				if(parameters.Count == 0)
					return Expect.From(body);
				return Expect.FromLambda(body, parameters);
			} catch(ExceptionExpressionException e) {
				var formatter = GetExpressionFormatter();
				return new NotExpectedExpect(e.Context, ConeMessage.Combine(
					ConeMessage.Parse($"{e.InnerException.Message}\nraised by '"),
					new[] {
						new ConeMessageElement(formatter.Format(e.Expression), "info"),
						new ConeMessageElement("'", string.Empty)
					}));
			} catch(NullSubexpressionException e) {
				var formatter = GetExpressionFormatter();
				return new NotExpectedExpect(e.Context, ConeMessage.Parse($"Null subexpression '{formatter.Format(e.Context)}' in\n'{formatter.Format(e.Expression)}'"));
			}
		}

		internal struct EvalResult
		{
			readonly object value;

			EvalResult(object value, bool isSuccess) { this.value = value; this.IsSuccess = isSuccess; }
			public static EvalResult Success(object value) => new EvalResult(value, true);
			public static EvalResult Failure(FailedExpectation value) => new EvalResult(value, false);

			public bool IsSuccess { get; }

			public object Value {
				get {
					if(IsSuccess)
						return value;
					throw new InvalidOperationException();
				}
			}

			public FailedExpectation Error {
				get {
					if(IsSuccess)
						throw new InvalidOperationException();
					return (FailedExpectation)value;
				}
			}
		}

		internal static EvalResult CheckExpect(IExpect expect) {
			var result = expect.Check();
			if (result.IsSuccess) 
				return EvalResult.Success(result.Actual.Value);

			return EvalResult.Failure(new FailedExpectation(
				ConeMessage.Combine(
					new [] {
						new ConeMessageElement(expect.FormatExpression(GetExpressionFormatter()), "expression"),
						ConeMessageElement.NewLine
					},
					expect.FormatMessage(ParameterFormatter)), 
				result.Actual, 
				result.Expected));
		}

		static EvalResult CheckExpect(Expression body, ExpressionEvaluatorParameters parameters) => 
			CheckExpect(ToExpect(body, parameters));

		static ExpressionFormatter GetExpressionFormatter() {
			var context = new StackTrace(true)
				.GetFrames()
				.Select(x => x.GetMethod())
				.First(x => x.Module.Assembly != typeof(Check).Assembly);
			return ExpressionFormatter.Rebind(context.DeclaringType);
		}

		public static CheckWith<T> With<T>(Expression<Func<T>> expr) {
			if(typeof(T).IsValueType)
				return new CheckWith<T>(expr.Body, (T)Evaluator.Evaluate(expr.Body, expr, ExpressionEvaluatorParameters.Empty).Result);
			var result = CheckExpect(Expression.NotEqual(expr.Body, Expression.Constant(null)), ExpressionEvaluatorParameters.Empty);
			if (result.IsSuccess)
				return new CheckWith<T>(expr.Body, (T)result.Value);
			throw MakeFail(result.Error, null);
		}

		internal class CheckWith
		{
			readonly object[] args;
			readonly Func<string> formatContext;

			static readonly Func<string> EmptyContext = () => string.Empty;

			public CheckWith(object[] args) : this(args, EmptyContext) { }

			public CheckWith(object[] args, Func<string> formatContext) {
				this.args = args;
				this.formatContext = formatContext;
			}

			public object That(LambdaExpression expr) {
				var result = BoundEval(expr);
				if (result.IsSuccess)
					return result.Value;
				throw MakeFail(new [] { result.Error });
			}

			public void That(IEnumerable<LambdaExpression> exprs) {
				var failed = GetFailed(exprs, BoundEval);
				if(failed.Length > 0)
					throw MakeFail(failed);
			}

			EvalResult BoundEval(LambdaExpression x) =>
				CheckExpect(x.Body, ExpressionEvaluatorParameters.Create(x.Parameters, args));

			Exception MakeFail(FailedExpectation[] failed) =>
				new CheckFailed(formatContext(), failed, null);
		}

		public class CheckWith<T>
		{
			readonly CheckWith check;

			public CheckWith(Expression context, T input) {
				this.check = new CheckWith(new object[]{ input }, () => ExpressionFormatter.Format(context));
			}

			public object That(Expression<Func<T,bool>> expr) => check.That(expr);

			public void That(Expression<Func<T,bool>> expr, params Expression<Func<T,bool>>[] extras) =>
				check.That(new [] { expr }.Concat(extras));
		}
	}

	public static class Check<TException> where TException : Exception
	{
		public static TException When(Expression<Action> expr) {
			var r = Check.CheckExpect(ExceptionExpect.From(expr, typeof(TException)));
			if(r.IsSuccess)
				return (TException)r.Value;
			throw Check.MakeFail(r.Error, null);
		}

		public static TException When<TValue>(Expression<Func<TValue>> expr) {
			var r = Check.CheckExpect(ExceptionExpect.From(expr, typeof(TException)));
			if(r.IsSuccess)
				return (TException)r.Value;
			throw Check.MakeFail(r.Error, null);
		}
	}

	//For those wanting to pre-warm their expectations.
	public static class Batteries
	{
		public static void Included() => Check.Initialize();
	}
}
