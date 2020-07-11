using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;
using CheckThat;
using CheckThat.Internals;
using CheckThat.Expectations;

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
            public CheckResult Check() {
                return new CheckResult(false, Maybe<object>.Some(42), Maybe<object>.None);
            }

            public string FormatExpression(IFormatter<Expression> formatter) {
                return "<expr>";
            }

            public string FormatActual(IFormatter<object> formatter) {
                return "<actual>";
            }

            public string FormatExpected(IFormatter<object> formatter) {
                return "<expected>";
            }

            public ConeMessage FormatMessage(IFormatter<object> formatter) {
                return ConeMessage.Parse("<message>");
            }
        }

        public void my_method_expect_provider_is_valid() {
            Check.That(() => ExpectFactory.IsMethodExpectProvider(typeof(MyIntegerMethodExpectProvider)));
        }

        public void obeys_result_and_formatting_from_expect() {
            var e = Check<Exception>.When(() => Check.That(() => new MyInteger(42).IsEven()));
            Check.That(() => e.Message == "<expr>\n<message>");
        }

        public void supports_static_method() {
            var e = Check<Exception>.When(() => Check.That(() => MyInteger.IsEven(42)));
            Check.That(() => e.Message == "<expr>\n<message>");
        }
    }
}
