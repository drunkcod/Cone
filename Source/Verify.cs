using System;
using System.Linq.Expressions;
using Cone.Core;
using Cone.Expectations;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Cone
{
    public class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();
        static ExpectFactory expect;
        public static Type Context;
        static ExpressionFormatter ExpressionFormatter = new ExpressionFormatter(typeof(Verify), ParameterFormatter);

        public static Func<IEnumerable<Assembly>> GetPluginAssemblies = () => AppDomain.CurrentDomain.GetAssemblies();

        static ExpectFactory Expect { get { return expect ?? (expect = new ExpectFactory(GetPluginAssemblies())); } }

        public static object That(Expression<Func<bool>> expr) {
            return Check(From(expr.Body));
        }

        public static void Guard(Action action) {
            try {
                action();
            } catch(TargetInvocationException e) {
                ExpectationFailed(e.InnerException.Message);
            } catch(Exception e) {
                ExpectationFailed(e.Message);
            }
        }

        public static class Throws<TException> where TException : Exception
        {
            public static TException When(Expression<Action> expr) {
                return (TException)Check(From(() => ExceptionExpect.From(expr, typeof(TException))));
            }

            public static TException When<TValue>(Expression<Func<TValue>> expr) {
                return (TException)Check(From(() => ExceptionExpect.From(expr, typeof(TException))));
            }
        }

        [Obsolete("use Verify.Throws<TException>.When(() => ...) instead")]
        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return Throws<TException>.When(expr);
        }

        static IExpect From(Expression body) {
            return From(() => Expect.From(body));
        }

        static IExpect From(Func<IExpect> build) {
            try {
                return build();
            } catch(ExceptionExpressionException e) {
                var formatter = GetExpressionFormatter();
                ExpectationFailed(string.Format("{0}\nraised by '{1}' in\n'{2}'", e.InnerException, formatter.Format(e.Expression), formatter.Format(e.Subexpression)));
            } catch(NullSubexpressionException e) {
                var formatter = GetExpressionFormatter();
                ExpectationFailed(string.Format("Null subexpression '{1}' in\n'{0}'", formatter.Format(e.Expression), formatter.Format(e.Context)));
            }
            return null;
        }
        
        static object Check(IExpect expect) {
            var result = expect.Check();
            if (!result.Success)
                ExpectationFailed(expect.FormatExpression(GetExpressionFormatter()) + "\n" + expect.FormatMessage(ParameterFormatter));
            return result.Actual;
        }       
 
        static ExpressionFormatter GetExpressionFormatter() { return ExpressionFormatter.Rebind(Context); }
    }
}
