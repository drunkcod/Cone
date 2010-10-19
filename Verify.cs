using System;
using System.Linq.Expressions;
using Cone.Expectations;

namespace Cone
{
    public class Verify
    {
        public static Action<string> ExpectationFailed = message => { throw new ExpectationFailedException(message); };
        static readonly ExpressionFormatter ExpressionFormatter = new ExpressionFormatter();
        static readonly ParameterFormatter ParameterFormatter = new ParameterFormatter();
        static readonly ExpectFactory Expect = new ExpectFactory();

        static IExpect From(Expression body) {
            return Expect.From(body);
        }

        public static object That(Expression<Func<bool>> expr) {
            return Check(From(expr.Body));
        }

        public static TException Exception<TException>(Expression<Action> expr) where TException : Exception {
            return (TException)Check(new ExceptionExpect(expr, typeof(TException)));
        }

        static object Check(IExpect expect) {
            object actual;
            if (!expect.Check(out actual))
                ExpectationFailed(expect.FormatExpression(ExpressionFormatter) + "\n" + expect.FormatMessage(ParameterFormatter));
            return actual;
        }        
    }
}
