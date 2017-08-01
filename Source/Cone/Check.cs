using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;
using System.Linq;

namespace Cone
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

		internal static void Initialize() {
			DoMakeFail = DefaultFail;
			That(() => Expect != null);
		}
		
		internal static Exception MakeFail(FailedExpectation fail, Exception innerException) => 
			DoMakeFail(string.Empty, new [] { fail }, innerException);

		internal delegate Exception NewFailedExpectationException(string context, FailedExpectation[] fails, Exception inner); 

		static NewFailedExpectationException DoMakeFail = (context, fail, inner) =>
			(DoMakeFail = LateBindFailureException())(context, fail, inner);

		static NewFailedExpectationException LateBindFailureException() {
			var nunit = Type.GetType("NUnit.Framework.AssertionException, NUnit.Framework");
			if (nunit == null)
				return DefaultFail;

			var fails = Expression.Parameter(typeof (IEnumerable<FailedExpectation>), "fails");
			var innerException = Expression.Parameter(typeof (Exception), "innerException");
			
			return Expression.Lambda<NewFailedExpectationException>(
				Expression.New(nunit.GetConstructor(new [] {typeof (string), typeof (Exception)}),
					Expression.Call(typeof (Check).GetMethod("FormatFailMessage", BindingFlags.Static | BindingFlags.NonPublic), fails),
					innerException),
				fails, innerException).Compile();
		}

		static Exception DefaultFail(string context, FailedExpectation[] fail, Exception innerException) =>
			new CheckFailed(context, fail, innerException);

		public static object That(Expression<Func<bool>> expr) {
			var result = CheckExpect(expr.Body, ExpressionEvaluatorParameters.Empty);
			if (result.IsSuccess)
				return result.Value;
			throw DoMakeFail(string.Empty, new [] { result.Error }, null);
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
			? DoMakeFail(string.Empty, failed, null) 
			: null;
		}

		static FailedExpectation[] GetFailed<T>(IEnumerable<T> xs, Func<T,EvalResult> check) =>
			xs.Select(check).Where(x => !x.IsSuccess).Select(x => x.Error).ToArray();

		public static TException Exception<TException>(Expression<Action> expr) where TException : Exception => Check<TException>.When(expr);

		class NotExpectedExpect : IExpect
		{
			readonly Expression expression;
			readonly ConeMessage message;

			public NotExpectedExpect(Expression expression, ConeMessage message) {
				this.expression = expression;
				this.message = message;
			}

			public CheckResult Check() => new CheckResult(false, Maybe<object>.None, Maybe<object>.None);

			public string FormatActual(IFormatter<object> formatter) => string.Empty;
			public string FormatExpected(IFormatter<object> formatter) => string.Empty;
			public string FormatExpression(IFormatter<Expression> formatter) => formatter.Format(expression);
			public ConeMessage FormatMessage(IFormatter<object> formatter) => message;
		}

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

		static EvalResult CheckExpect(Expression body, ExpressionEvaluatorParameters parameters) => CheckExpect(ToExpect(body, parameters));

		static ExpressionFormatter GetExpressionFormatter() {
			var context = new StackTrace().GetFrames()
				.Select(x => x.GetMethod()).First(x => x.DeclaringType.Assembly != typeof(Check).Assembly);
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

		public class CheckWith
		{
			readonly object[] args;

			protected CheckWith(params object[] args) {
				this.args = args;
			}

			internal EvalResult BoundEval(LambdaExpression x) =>
				CheckExpect(x.Body, ExpressionEvaluatorParameters.Create(x.Parameters, args));

			protected virtual string FormatContext() => string.Empty;

			protected Exception MakeFail(FailedExpectation[] failed) => DoMakeFail(FormatContext(), failed, null);

			protected object ThatCore(LambdaExpression expr) {
				var result = BoundEval(expr);
				if (result.IsSuccess)
					return result.Value;
				throw MakeFail(new [] { result.Error });
			}

			protected void ThatCore(IEnumerable<LambdaExpression> exprs) {
				var failed = GetFailed(exprs, BoundEval);
				if(failed.Length> 0)
					throw MakeFail(failed);
			}
		}

		public class CheckWith<T> : CheckWith
		{
			readonly Expression context;

			public CheckWith(Expression context, T input) : base(input) {
				this.context = context;
			}

			public object That(Expression<Func<T,bool>> expr) => ThatCore(expr);

			public void That(Expression<Func<T,bool>> expr, params Expression<Func<T,bool>>[] extras) =>
				ThatCore(new [] { expr }.Concat(extras));

			protected override string FormatContext() => ExpressionFormatter.Format(context);
		}

		internal class CheckWith<T0, T1> : CheckWith
		{
			public CheckWith(T0 arg0, T1 arg1) : base(arg0, arg1) { }

			public void That(Expression<Func<T0, T1,bool>> expr, params Expression<Func<T0, T1,bool>>[] extras) =>
				ThatCore(new [] { expr }.Concat(extras));
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
}
