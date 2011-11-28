using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;

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

            public static bool IsEven(int value) { return new MyInteger(value).IsEven(); }

            public bool IsEven() { return true; }
        }

        public class MyIntegerMethodExpectProvider : IMethodExpectProvider
        {
            public IEnumerable<MethodInfo> GetSupportedMethods() {
                return typeof(MyInteger).GetMethods().Where(x => x.Name == "IsEven");
            }

            public IExpect GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
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

            public string FormatActual(IFormatter<object> formatter) {
                return "<actual>";
            }

            public string FormatExpected(IFormatter<object> formatter) {
                return "<expected>";
            }

            public string FormatMessage(IFormatter<object> formatter) {
                return "<message>";
            }
        }

        public void my_method_expect_provider_is_valid() {
            Verify.That(() => ExpectFactory.IsMethodExpectProvider(typeof(MyIntegerMethodExpectProvider)));
        }

        public void obeys_result_and_formatting_from_expect() {
            var e = Verify.Throws<Exception>.When(() => Verify.That(() => new MyInteger(42).IsEven()));
            Verify.That(() => e.Message == "<expr>\n<message>");
        }

        public void supports_static_method() {
            var e = Verify.Throws<Exception>.When(() => Verify.That(() => MyInteger.IsEven(42)));
            Verify.That(() => e.Message == "<expr>\n<message>");
        }
    }
}
