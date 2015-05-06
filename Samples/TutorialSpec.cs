using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Cone.Samples
{
	[Describe(typeof(TutorialSpec))]
	public class TutorialSpec
	{
		[Explicit]
		public void this_test_should_be_explicit() {          
			Check.That(() => false); 
		}

		[Row(typeof(object)), DisplayAs("{0}", Heading = "Format types")]
		public void Format_types(Type type)
		{ }

		[Context("null subexpression detection")]
		public class NullSubexpressionDetection
		{
			public void member_access_would_raise_NullReferenceException() {
				var foo = new { ThisValueIsNull = (string)null };
				Check.That(() => foo.ThisValueIsNull.Length != 0); 
			}
			public void method_call_would_raise_NullReferenceException() {
				var foo = new { ThisValueIsNull = (string)null };
				Check.That(() => foo.ThisValueIsNull.Contains("hello")); 
			}
		}

		[Context("DisplayAs")]
		public class DisplayAs
		{
			[DisplayAs("{0} + {1} == {2}")
			,Row(1, 2, 3)]
			public void Add(int a, int b, int result) {
				Check.That(() => a + b == result);
			}

			[DisplayAs("{0} - {1} == {2}", Heading = "When subtracting {1} from {0} the we get {2}")
			,Row(3, 2, 1)]
			public void Subtract(int a, int b, int result) {
				Check.That(() => a - b == result);
			}

			public enum MyEnum { Zero, One }
			[Row(MyEnum.Zero)]
			public void enums(MyEnum value) { }

			[Row(MyEnum.Zero), DisplayAs("{0}")]
			public void formatted_enums(MyEnum value) { Check.That(() => value == MyEnum.One); }
		}

		public void report_failing_subexpression()
		{
			Func<int> throws = () => { throw new InvalidOperationException(); }; 
			Check.That(() => throws() == 42); 
		}

		public void report_failing_subexpression_member_access() {
			Func<string> throws = () => { throw new InvalidOperationException(); }; 

			Check.That(() => throws().Length == 42); 
		}

		int Throws() { throw new NotImplementedException(); }
		
		public void wrong_subexpression_raises_exception() {
			Func<TutorialSpec> getTarget = () => { throw new InvalidOperationException(); };
			Check<NotImplementedException>.When(() => getTarget().Throws());
		}

		public void report_failing_subexpression_call()
		{
			Check.That(() => Throws() == 42); 
		}

		public void funky(Func<int, int> fun, int input, int result) {
			if(fun != null)
				Check.That(() => fun(input) == result);
		}

		public IEnumerable<IRowTestData> FunkyRows() {
			return new RowBuilder<TutorialSpec>()
				.Add(x => x.funky(input => input + 1, 1, 2))
				.Add(x => x.funky(null, 0, 0));
		}

		[Context("when using fluent interfaces")]
		public class FluentInterfaces 
		{
			public class MyDsl 
			{
				public class Stuff {
					public string ThisPropertyThrows { get { throw new NotImplementedException(); } }
					public string ThisThrows() { throw new NotImplementedException(); }
				}

				public Stuff TakeItToAnohterLevel { get { return new Stuff(); } }
			}

			public void when_things_throw() {
				Check.That(() => new MyDsl.Stuff().ThisThrows().Length == 0);

			}

			public void when_nested_things_throw() {
				Check.That(() => new MyDsl().TakeItToAnohterLevel.ThisThrows() == "");
			}
		}
	}

	[Feature("More failures")]
	public class MoreFailures
	{
		[Pending]
		public void pending_tests_fail_when_passing() { }

		[Context("BeforeEach failure")]
		public class BeforeEachFails
		{
			[BeforeEach]
			public void DieDieDie(){ throw new Exception("BeforeEach failure"); }
			public void test() {}
		}
		[Context("AfterEach failure")]
		public class AfterEachFails
		{
			[AfterEach]
			public void DieDieDie(){ throw new Exception("AfterEach failure"); }
			public void test() {}
		}
		[Context("Before & After failure")]
		public class EpicFail
		{
			[BeforeEach, AfterEach]
			public void DieDieDie(){ throw new Exception("Epic failure"); }
			public void test() {}
		}
	}
}
