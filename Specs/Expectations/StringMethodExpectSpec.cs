using System;
using System.Linq.Expressions;
using Cone.Core;

namespace Cone.Expectations
{
    class NullFormatter : IFormatter<object>
    {
        public string Format(object value) {
            return value.ToString();
        }
    }

    [Describe(typeof(StringMethodExpect))]
    public class StringMethodExpectSpec
    {
        public void EndsWith() {
            Verify.That(() => Expected(() => "s".EndsWith("value")) == "a string ending with value");
        }

        public void EndsWith_using_specific_comparision() {
            Verify.That(() => Expected(() => "s".EndsWith("value", StringComparison.CurrentCulture)) == "a string ending with value using 'CurrentCulture'");
        }

        public void message_formatting() {
            Verify.That(() => Expected(() => "s".Contains("value")) == "a string containing value");
        }


        string Expected(Expression<Func<bool>> expression) {
            IMethodExpectProvider provider = new StringMethodsProvider();
            var eval = new ExpressionEvaluator();
            var call = (MethodCallExpression)expression.Body;
            var expect = provider.GetExpectation(call, call.Method, 
                eval.EvaluateAsTarget(call.Object, call).Value,
                (object[])eval.EvaluateAll(call.Arguments, call).Value);
            return expect.FormatExpected(new NullFormatter());
        }
    }
}
