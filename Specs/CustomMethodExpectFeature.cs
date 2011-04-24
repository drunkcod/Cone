using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Expectations;
using System.Linq.Expressions;

namespace Cone
{
    [Feature("Custom IMethodExpectProvider")]
    public class CustomMethodExpectFeature
    {
        public class MyInteger 
        {
            readonly int value;

            public MyInteger(int value) {
                this.value = value;
            }

            public bool IsEven() { return true; }
        }

        public class MyIntegerMethodExpectProvider : IMethodExpectProvider
        {
            public IEnumerable<MethodInfo> GetSupportedMethods() {
                return new[]{ typeof(MyInteger).GetMethod("IsEven") };
            }

            public IExpect GetExpectation(Expression body, MethodInfo method, object target, IEnumerable<object> args) {
                return new MyIntegerExpect();
            }
        }

        public class MyIntegerExpect : IExpect
        {
            public ExpectResult Check() {
                return new ExpectResult {
                    Actual = 42,
                    Success = false
                };
            }

            public string FormatExpression(IFormatter<System.Linq.Expressions.Expression> formatter) {
                return "<expr>";
            }

            public string FormatMessage(IFormatter<object> formatter) {
                return "<message>";
            }
        }

        public void my_method_expect_provider_is_valid() {
            Verify.That(() => ExpectFactory.IsMethodExpectProvider(typeof(MyIntegerMethodExpectProvider)));
        }

        public void obeys_result_and_formatting_from_exepct() {
            var e = Verify.Throws<Exception>.When(() => Verify.That(() => new MyInteger(42).IsEven()));
            Verify.That(() => e.Message == "<expr>\n<message>");
        }
    }
}
