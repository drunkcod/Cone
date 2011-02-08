using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    public class StringContainsProvider : IMethodExpectProvider
    {
        IEnumerable<MethodInfo> IMethodExpectProvider.GetSupportedMethods() {
            return new[]{ typeof(string).GetMethod("Contains") };
        }

        IExpect IMethodExpectProvider.GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
            return new StringContainsExpect(body, target.ToString(), args[0].ToString());
        }
    }

    public class StringContainsExpect : IExpect
    {
        readonly Expression body;
        readonly string actual;
        readonly string value;

        public StringContainsExpect(Expression body, string actual, string value) {
            this.body = body;
            this.actual = actual;
            this.value = value;
        }

        ExpectResult IExpect.Check() {
            return new ExpectResult { 
                Actual = actual, 
                Success = actual.Contains(value)
            };
        }

        string IExpect.FormatExpression(IFormatter<System.Linq.Expressions.Expression> formatter) {
            return formatter.Format(body);
        }

        string IExpect.FormatMessage(IFormatter<object> formatter) {
            return string.Format(ExpectMessages.EqualFormat, formatter.Format(actual), "string containing " + formatter.Format(value));
        }
    }
}
