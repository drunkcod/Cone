using System;
using System.Linq.Expressions;
using Cone.Core;
using System.Globalization;

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

        public void EndsWith_using_case_sensativity_and_culture() {
            Verify.That(() => Expected(() => "s".EndsWith("value", false, CultureInfo.CurrentCulture)) == string.Format("a string ending with value using '{0}'", CultureInfo.CurrentCulture));
        }

        public void EndsWith_ignoreing_case_using_culture() {
            Verify.That(() => Expected(() => "s".EndsWith("value", true, CultureInfo.InvariantCulture)) == "a string ending with value using 'InvariantCulture' & ignoring case");
        }

        public void Contains() {
            Verify.That(() => Expected(() => "s".Contains("value")) == "a string containing value");
        }

        string Expected(Expression<Func<bool>> expression) {
            IMethodExpectProvider provider = new StringMethodsProvider();
            var eval = new ExpressionEvaluator();
            var call = (MethodCallExpression)expression.Body;
            var expect = provider.GetExpectation(call, call.Method, 
                eval.EvaluateAsTarget(call.Object, call).Result,
                (object[])eval.EvaluateAll(call.Arguments, call).Result);
            return expect.FormatExpected(new NullFormatter());
        }
    }
}
