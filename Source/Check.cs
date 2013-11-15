using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;
using System.Linq;

namespace Cone
{
    public static class Check
    {
        public static Action<IEnumerable<FailedExpectation>, Exception> ExpectationFailed = (fails, innerException) => { 
			throw new ExpectationFailedException(fails, innerException); };
        static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();
        static ExpectFactory expect;

        [ThreadStatic]
        public static Type Context;

        static readonly ExpressionFormatter ExpressionFormatter = new ExpressionFormatter(typeof(Check), ParameterFormatter);

        public static Func<IEnumerable<Assembly>> GetPluginAssemblies = () => AppDomain.CurrentDomain.GetAssemblies();

        static ExpectFactory Expect { get { return expect ?? (expect = new ExpectFactory(GetPluginAssemblies())); } }

        static internal void Initialize() {
            Check.That(() => 1 == 1);
        }

        public static object That(Expression<Func<bool>> expr) {
			return Eval(ToExpect(expr.Body), Fail);
        }

		public static void Fail(FailedExpectation fail, Exception innerException) {
			ExpectationFailed(new[]{ fail }, innerException);
		}

        public static void That(Expression<Func<bool>> expr, params Expression<Func<bool>>[] extras) {
			That(new[]{ expr }.Concat(extras));
        }

		public static void That(IEnumerable<Expression<Func<bool>>> exprs) {
			var failed = new List<FailedExpectation>(); 
			exprs.ForEach(x => Eval(ToExpect(x.Body), (fail, _) => failed.Add(fail)));
			if(failed.Count != 0)
				ExpectationFailed(failed, null);
		}

        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return Check<TException>.When(expr);
        }

        static IExpect ToExpect(Expression body) {
            return ToExpect(() => Expect.From(body));
        }

        internal static IExpect ToExpect(Func<IExpect> build) {
            try {
                return build();
            } catch(ExceptionExpressionException e) {
                var formatter = GetExpressionFormatter();
                Fail(new FailedExpectation(string.Format("{0}\nraised by '{1}' in\n'{2}'", e.InnerException.Message, formatter.Format(e.Expression), formatter.Format(e.Subexpression)), Maybe<object>.None, Maybe<object>.None), e);
            } catch(NullSubexpressionException e) {
                var formatter = GetExpressionFormatter();
                Fail(new FailedExpectation(string.Format("Null subexpression '{1}' in\n'{0}'", formatter.Format(e.Expression), formatter.Format(e.Context)), Maybe<object>.None, Maybe<object>.None), e);
            }
            return null;
        }
        
        internal static object Eval(IExpect expect, Action<FailedExpectation, Exception> onFail) {
            var result = expect.Check();
            if (!result.IsSuccess)
                onFail(new FailedExpectation(expect.FormatExpression(GetExpressionFormatter()) + "\n" + expect.FormatMessage(ParameterFormatter), result.Actual, result.Expected), null);
            return result.Actual.Value;
        }       
 
        static ExpressionFormatter GetExpressionFormatter() { return ExpressionFormatter.Rebind(Context); }
    }

	public static class Check<TException> where TException : Exception
    {
        public static TException When(Expression<Action> expr) {
            return (TException)Check.Eval(Check.ToExpect(() => ExceptionExpect.From(expr, typeof(TException))), Check.Fail);
        }

        public static TException When<TValue>(Expression<Func<TValue>> expr) {
            return (TException)Check.Eval(Check.ToExpect(() => ExceptionExpect.From(expr, typeof(TException))), Check.Fail);
        }
    }

}
