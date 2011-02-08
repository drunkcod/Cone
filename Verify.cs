using System;
using System.Linq.Expressions;
using Cone.Expectations;
using System.Reflection;
using System.Diagnostics;

namespace Cone
{
    public class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();
        static readonly ExpectFactory Expect = new ExpectFactory();
        static internal Type Context;
        static ExpressionFormatter ExpressionFormatter = new ExpressionFormatter(typeof(Verify), ParameterFormatter);

        static IExpect From(Expression body) {
            return Expect.From(body);
        }

        public static object That(Expression<Func<bool>> expr) {
            
            return Check(From(expr.Body));
        }

        public static class Throws<TException> where TException : Exception
        {
            public static TException When(Expression<Action> expr) {
                return (TException)Check(ExceptionExpect.From(expr, typeof(TException)));
            }

            public static TException When<TValue>(Expression<Func<TValue>> expr) {
                return (TException)Check(ExceptionExpect.From(expr, typeof(TException)));
            }
        }

        [Obsolete("use Verify.Throws<TException>.When(() => ...) instead")]
        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return Throws<TException>.When(expr);
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
