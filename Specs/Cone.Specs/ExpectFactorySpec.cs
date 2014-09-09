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
		ExpectFactory Expectory = new ExpectFactory(new[]{ typeof(CustomMethodExpectProviders).Assembly, typeof(Check).Assembly });
		int A = 1, B = 2;

		public void string_equality() {
			string a = "a", b = "b";

			Check.That(() => ExpectFrom(() => a == b) is StringEqualExpect);
		}

		[DisplayAs("\"is\" test with null")]
		public void type_check_null() {
			object obj = null;

			Check.That(() => ExpectFrom(() => obj is string) is TypeIsExpect);
		}

		class Base { }
		class Derived : Base { }

		[DisplayAs("\"is\" with inheritance")]
		public void type_check_with_inheritance() {
			Check.That(() => new Derived() is Base);
		}

		[DisplayAs("A == B")]
		public void EqualExpect() {
			Check.That(() => ExpectFrom(() => A == B) is EqualExpect);
		}

		[DisplayAs("A != B")]
		public void NotEqualExpect() {
			Check.That(() => ExpectFrom(() => A != B) is NotEqualExpect);
		}

		[DisplayAs("A < B")]
		public void LessThanExpect() {
			Check.That(() => ExpectFrom(() => A < B) is LessThanExpect);
		}

		[DisplayAs("A <= B")]
		public void LessThanOrEqualExpect() {
			Check.That(() => ExpectFrom(() => A <= B) is LessThanOrEqualExpect);
		}

		[DisplayAs("A > B")]
		public void GreaterThanExpect() {
			Check.That(() => ExpectFrom(() => A > B) is GreaterThanExpect);
		}

		[DisplayAs("A >= B")]
		public void GreaterThanOrEqualExpect() {
			Check.That(() => ExpectFrom(() => A >= B) is GreaterThanOrEqualExpect);
		}

		[DisplayAs("String.Contains(\"Value\")")]
		public void StringContains() {
			Check.That(() => ExpectFrom(() => "Hello".Contains("World")) is StringMethodExpect);
		}

		enum MyEnum { Value };
		public void enum_equals() {
			var actual = new { Value = MyEnum.Value };
			var eq = (EqualExpect)Check.That(() => ExpectFrom(() => actual.Value == MyEnum.Value) is EqualExpect);
			Check.That(
				() => eq.FormatActual(new NullFormatter()) == "Value",
				() => eq.FormatExpected(new NullFormatter()) == "Value");
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
				Check.That(() => ExpectFactory.IsMethodExpectProvider(typeof(NestedProvider)));
			}
		}
	}
}