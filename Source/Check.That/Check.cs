using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CheckThat.Expectations;
using CheckThat.Expressions;
using CheckThat.Formatting;
using CheckThat.Internals;
using Cone;

namespace CheckThat
{
	/*
	 * This file contains more repitition than any sane man would normally agree with.
	 * The not so good reason for it is to keep stack frames cleaner for test runners that simply spew
	 * out the complete frame without attempting to prettify it.
	 */
	public static class Check
	{
		static ExpectFactory expect;

		static readonly ExpressionEvaluator Evaluator = new ExpressionEvaluator();
		static ExpectFactory Expect => expect ??= new ExpectFactory(Evaluator, GetPluginAssemblies());
		static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();
		static readonly ExpressionFormatter ExpressionFormatter = new ExpressionFormatter(typeof(Check), ParameterFormatter);

		public static Func<IEnumerable<Assembly>> GetPluginAssemblies = GetDefaultPluginAssemblies;

		static IEnumerable<Assembly> GetDefaultPluginAssemblies() =>
			AppDomain.CurrentDomain.GetAssemblies().Where(ReferencesExtensionPoints);

		public static bool IncludeGuide 
		{
			get { return Expect.IncludeGuide; } 
			set { Expect.IncludeGuide = value; }
		}

		static bool ReferencesExtensionPoints(Assembly assembly) => 
			assembly.GetReferencedAssemblies().Any(a => a.FullName == ExtensionsAssembly.FullName);

		static Assembly ExtensionsAssembly => typeof(IMethodExpectProvider).Assembly;
		
		internal static Exception MakeFail(FailedExpectation fail) =>
			new CheckFailed(string.Empty, fail, null);

		internal static Exception MakeFail(FailedExpectation[] fails) =>
			new CheckFailed(string.Empty, fails, null);

		public static object That(Expression<Func<bool>> expr) {
			var result = CheckExpect(expr.Body, ExpressionEvaluatorParameters.Empty);
			if (result.IsSuccess)
				return result.Value;
			throw MakeFail(result.Error);
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
			return failed != null ? MakeFail(failed) : null;
		}

		static FailedExpectation[] GetFailed<T>(IEnumerable<T> xs, Func<T,EvalResult> check) {
			List<FailedExpectation> errors = null;
			foreach(var item in xs) {
				var r = check(item);
				if(!r.IsSuccess)
					(errors ??= new List<FailedExpectation>()).Add(r.Error);
			}
			return errors?.ToArray();
		}

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
			public object Value => IsSuccess ? value : throw new InvalidOperationException();
			public FailedExpectation Error => IsSuccess ? throw new InvalidOperationException() : (FailedExpectation)value;
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
			throw MakeFail(result.Error);
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
				if(failed != null)
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

		internal static void Initialize() => That(() => Expect != null);
	}

	public static class Check<TException> where TException : Exception
	{
		public static TException When(Expression<Action> expr) {
			var r = Check.CheckExpect(ExceptionExpect.From(expr, typeof(TException)));
			if(r.IsSuccess)
				return (TException)r.Value;
			throw Check.MakeFail(r.Error);
		}

		public static TException When<TValue>(Expression<Func<TValue>> expr) {
			var r = Check.CheckExpect(ExceptionExpect.From(expr, typeof(TException)));
			if(r.IsSuccess)
				return (TException)r.Value;
			throw Check.MakeFail(r.Error);
		}
	}

	//For those wanting to pre-warm their expectations.
	public static class Batteries
	{
		public static void Included() => Check.Initialize();
	}
}
