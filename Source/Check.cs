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
	 * This file contains more repitition than any sane mane would normally agree with.
	 * The not so good reason for it is to keep stack frames cleaner for test runners that simply spew
	 * out the complete frame without attempting to prettify it.
	 */

    public static class Check
    {
        static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();
        static ExpectFactory expect;

        static readonly ExpressionFormatter ExpressionFormatter = new ExpressionFormatter(typeof(Check), ParameterFormatter);

        public static Func<IEnumerable<Assembly>> GetPluginAssemblies = () => AppDomain.CurrentDomain.GetAssemblies();

        static ExpectFactory Expect { get { return expect ?? (expect = new ExpectFactory(GetPluginAssemblies())); } }

        static internal void Initialize() {
	        DoMakeFail = DefaultFail;
            That(() => 1 == 1);
        }

	    internal static Exception MakeFail(FailedExpectation fail, Exception innerException) {
		    return DoMakeFail(new[] { fail }, innerException);
	    }

	    private static Func<IEnumerable<FailedExpectation>, Exception, Exception> DoMakeFail = (fail, inner) =>
	    {
			var nunit = Type.GetType("NUnit.Framework.AssertionException, NUnit.Framework");
		    if (nunit == null)
			    DoMakeFail = DefaultFail;
		    else
		    {
			    var message = Expression.Parameter(typeof(string), "message");
			    var innerException = Expression.Parameter(typeof (Exception), "innerException");
			    var lambda = Expression.Lambda<Func<string, Exception, Exception>>(
				    Expression.New(nunit.GetConstructor(new[] {typeof (string), typeof (Exception)}),
					    new[] {message, innerException}), message, innerException);

				var baked = lambda.Compile();
			    DoMakeFail = (f, i) => baked(string.Join("\n", f.Select(x => x.Message).ToArray()), i);
		    }
			return DoMakeFail(fail, inner);
	    };

	    static Exception DefaultFail(IEnumerable<FailedExpectation> fail, Exception innerException) {
		    return new CheckFailed(fail, innerException);		    
	    }

	    public static object That(Expression<Func<bool>> expr) {
	        object result;
		    if (TryEval(ToExpect(expr.Body), out result))
			    return result;
	        throw MakeFail((FailedExpectation)result, null);
	    }

		public static void That(Expression<Func<bool>> expr, params Expression<Func<bool>>[] extras) {
			That(new[]{ expr }.Concat(extras));
        }

		public static void That(IEnumerable<Expression<Func<bool>>> exprs) {
			var failed = new List<FailedExpectation>();
			exprs.ForEach(x => {
				object r;
				if(!TryEval(ToExpect(x.Body), out r))
					failed.Add((FailedExpectation)r);
			});
			if(failed.Count != 0)
				throw DoMakeFail(failed, null);
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
	            var fail = new FailedExpectation(string.Format("{0}\nraised by '{1}' in\n'{2}'", e.InnerException.Message, formatter.Format(e.Expression), formatter.Format(e.Subexpression)), Maybe<object>.None, Maybe<object>.None);
	            throw MakeFail(fail, e);
            } catch(NullSubexpressionException e) {
                var formatter = GetExpressionFormatter();
	            var fail = new FailedExpectation(string.Format("Null subexpression '{1}' in\n'{0}'", formatter.Format(e.Expression), formatter.Format(e.Context)), Maybe<object>.None, Maybe<object>.None);
	            throw MakeFail(fail, e);
            }
        }

        internal static bool TryEval(IExpect expect, out object r) {
            var result = expect.Check();
	        if (result.IsSuccess) {
		        r = result.Actual.Value;
		        return true;
	        }
            r = new FailedExpectation(expect.FormatExpression(GetExpressionFormatter()) + "\n" + expect.FormatMessage(ParameterFormatter), result.Actual, result.Expected);
	        return false;
        }

	    static ExpressionFormatter GetExpressionFormatter() {
		    var frames = new StackTrace(1);
		    var context = frames.GetFrames().Select(x => x.GetMethod()).First(x => x.DeclaringType != typeof (Check));
		    return ExpressionFormatter.Rebind(context.DeclaringType);
	    }
    }

	public static class Check<TException> where TException : Exception
    {
        public static TException When(Expression<Action> expr) {
	        object r;
			if(Check.TryEval(Check.ToExpect(() => ExceptionExpect.From(expr, typeof(TException))), out r))
	            return (TException)r;
	        throw Check.MakeFail((FailedExpectation)r, null);
        }

        public static TException When<TValue>(Expression<Func<TValue>> expr) {
	        object r;
			if(Check.TryEval(Check.ToExpect(() => ExceptionExpect.From(expr, typeof(TException))), out r))
	            return (TException)r;
	        throw Check.MakeFail((FailedExpectation)r, null);
        }
    }
}
