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

        static IExpect From(Expression body) {
            return Expect.From(body);
        }

        public static object That(Expression<Func<bool>> expr) {
            
            return Check(From(expr.Body), 1);
        }

        public static class Throws<TException> where TException : Exception
        {
            public static TException When(Expression<Action> expr) {
                return (TException)Check(ExceptionExpect.From(expr, typeof(TException)), 1);
            }

            public static TException When<TValue>(Expression<Func<TValue>> expr) {
                return (TException)Check(ExceptionExpect.From(expr, typeof(TException)), 1);
            }
        }

        [Obsolete("use Verify.Throws<TException>.When(() => ...) instead")]
        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return Throws<TException>.When(expr);
        }

        static object Check(IExpect expect, int skipFrames) {
            object actual;
            if (!expect.Check(out actual))
                ExpectationFailed(expect.FormatExpression(GetExpressionFormatter(GetCallingType(1 + skipFrames))) + "\n" + expect.FormatMessage(ParameterFormatter));
            return actual;
        }       
 
        static Type GetCallingType(int skipFrames) {
            return new StackFrame(1 + skipFrames).GetMethod().DeclaringType;
        }

        static ExpressionFormatter GetExpressionFormatter(Type context) {
 
            return new ExpressionFormatter(context); 
        }
    }
}
