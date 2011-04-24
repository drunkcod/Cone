using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Expectations;

namespace Cone
{
    [Describe(typeof(ExpectFactory))]
    public class ExpectFactorySpec
    {
        ExpectFactory Expectory = new ExpectFactory();
        int A = 1, B = 2;

        public void string_equality() {
            string a = "a", b = "b";

            Verify.That(() => ExpectFrom(() => a == b) is StringEqualExpect);
        }

        [DisplayAs("\"is\" test")]
        public void type_check() {
            var obj = new object();

            Verify.That(() => ExpectFrom(() => obj is string) is TypeIsExpect);
            
            var expect = (Expect)ExpectFrom(() => obj is string);
            Verify.That(() => (Type)expect.Actual == typeof(object));
            Verify.That(() => (Type)expect.Expected == typeof(string));
        }

        class Base { }
        class Derived : Base { }

        [DisplayAs("\"is\" with inheritance")]
        public void type_check_with_inheritance() {
            Verify.That(() => new Derived() is Base);
        }

        [DisplayAs("A == B")]
        public void EqualExpect() {
            Verify.That(() => ExpectFrom(() => A == B) is EqualExpect);
        }

        [DisplayAs("A != B")]
        public void NotEqualExpect() {
            Verify.That(() => ExpectFrom(() => A != B) is NotEqualExpect);
        }

        [DisplayAs("A < B")]
        public void LessThanExpect() {
            Verify.That(() => ExpectFrom(() => A < B) is LessThanExpect);
        }

        [DisplayAs("A <= B")]
        public void LessThanOrEqualExpect() {
            Verify.That(() => ExpectFrom(() => A <= B) is LessThanOrEqualExpect);
        }

        [DisplayAs("A > B")]
        public void GreaterThanExpect() {
            Verify.That(() => ExpectFrom(() => A > B) is GreaterThanExpect);
        }

        [DisplayAs("A >= B")]
        public void GreaterThanOrEqualExpect() {
            Verify.That(() => ExpectFrom(() => A >= B) is GreaterThanOrEqualExpect);
        }

        [DisplayAs("String.Contains(\"Value\")")]
        public void StringContains() {
            Verify.That(() => ExpectFrom(() => "Hello".Contains("World")) is StringMethodExpect);
        }

        IExpect ExpectFrom(Expression<Func<bool>> expression) {
            return Expectory.From(expression.Body);
        }

        [Context("detecting custom IMethodExpect providers")]
        public class CustomMethodExpectProviders 
        {
            public class NestedProvider : IMethodExpectProvider 
            {
                IEnumerable<MethodInfo> IMethodExpectProvider.GetSupportedMethods() {
                    return new MethodInfo[0];
                }

                IExpect IMethodExpectProvider.GetExpectation(Expression body, System.Reflection.MethodInfo method, object target, object[] args) {
                    throw new NotImplementedException();
                }
            }

            public void public_nested_provider_is_detected() {
                Verify.That(() => ExpectFactory.IsMethodExpectProvider(typeof(NestedProvider)));
            }
        }
    }
}
