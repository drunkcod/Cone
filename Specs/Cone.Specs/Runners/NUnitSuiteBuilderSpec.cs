using System;
using System.Linq;
using NUnit.Framework;

//mimic the NUnit framework attributes, matches must be name based to avoid referenceing nunit.
namespace NUnit.Framework
{
	public class TestFixtureAttribute : Attribute { }

	public class TestFixtureSetUpAttribute : Attribute { }

	public class SetUpAttribute : Attribute { }

	public class TearDownAttribute : Attribute { }

	public class TestFixtureTearDownAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CategoryAttribute : Attribute 
	{
		readonly string @name;

		public CategoryAttribute(string name) {
			this.@name = name;
		}

 		public string Name { get { return @name; } }
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class TestCaseAttribute : Attribute
	{
		readonly object[] arguments;

		public TestCaseAttribute(params object[] arguments) {
			this.arguments = arguments;
		}

		public object[] Arguments { get { return arguments; } }
		public object Result { get; set; }
		public string TestName { get; set; }
	}
}

namespace Cone.Runners
{
	[Describe(typeof(NUnitSuiteBuilder))]
	public class NUnitSuiteBuilderSpec
	{
		[TestFixture, Category("SomeCategory"), Category("Integration")]
		class MyNUnitFixture
		{ 
			public int Calls;
			public int FixtureSetUpCalled;
			public int SetUpCalled;
			public int TestCalled;
			public int TearDownCalled;
			public int FixtureTearDownCalled;

			[TestFixtureSetUp]
			public void FixtureSetUp() { FixtureSetUpCalled = ++Calls; }

			[SetUp]
			public void SetUp() { SetUpCalled = ++Calls; }

			public void a_test(){ TestCalled = ++Calls; }

			public int NotATest() { return 42; }


			[TearDown]
			public void TearDown() { TearDownCalled = ++Calls; }

			[TestFixtureTearDown]
			public void FixtureTearDown() { FixtureTearDownCalled = ++Calls; }
		}

		NUnitSuiteBuilder SuiteBuilder = new NUnitSuiteBuilder();

		public void supports_building_suites_for_types_with_NUnit_TestFixture_attribute() {
			Verify.That(() => SuiteBuilder.SupportedType(typeof(MyNUnitFixture)));
		}

		[Context("given description of MyNunitFixture")]
		public class NUnitSuiteBuilderFixtureDescriptionSpec
		{
			IFixtureDescription Description; 

			[BeforeAll]
			public void Given_description_of_MyNUnitFixture()
			{
				var suiteBuilder = new NUnitSuiteBuilder();
				Description = suiteBuilder.DescriptionOf(typeof(MyNUnitFixture));
			}

			public void suite_type_is_TestFixture() {
				Verify.That(() => Description.SuiteType == "TestFixture");
			}

			public void test_name_is_name_of_fixture() {
				Verify.That(() => Description.TestName == typeof(MyNUnitFixture).Name);
			}

			public void suite_name_is_namespace_of_fixture() {
				Verify.That(() => Description.SuiteName == typeof(MyNUnitFixture).Namespace);
			}

			public void categories_found_via_Category_attribute() {
				Verify.That(() => Description.Categories.Contains("SomeCategory"));
				Verify.That(() => Description.Categories.Contains("Integration"));
			}
		}

		[Context("given a fixture instance")]
		public class NUnitSuiteBuilderFixtureInstanceSpec
		{
			private MyNUnitFixture NUnitFixture;
			private ConePadSuite NUnitSuite;

			[BeforeEach]
			public void GivenFixtureInstance() {
				NUnitSuite = new NUnitSuiteBuilder().BuildSuite(typeof(MyNUnitFixture)); 
				NUnitFixture = (MyNUnitFixture)NUnitSuite.Fixture;
				NUnitSuite.Run(new TestSession(new NullLogger()));
			}

			public void FixtureSetUp_is_called_to_initialize_fixture() {
				Verify.That(() => NUnitFixture.FixtureSetUpCalled == 1);
			}

			public void SetUp_is_called_as_BeforeEach() {
				Verify.That(() => NUnitFixture.SetUpCalled == NUnitFixture.TestCalled - 1);
			}
			
			public void TearDown_is_called_as_AfterEach() {
				Verify.That(() => NUnitFixture.TearDownCalled == NUnitFixture.TestCalled + 1);
			}

			public void TestFixtureTearDown_is_used_to_release_fixture() {
				Verify.That(() => NUnitFixture.FixtureTearDownCalled == NUnitFixture.Calls);
			}
		}

		[Context("given fixture with TestCase's")]
		public class NUnitSuiteBuilderTestCaseSpec
		{
			class FixtureWithTestCases
			{
				[TestCase(1, 2, Result = 3, TestName = "1 + 2 = 3")]
				[TestCase(1, 1, Result = 3, TestName = "1 + 1 = 3")]
				public int add(int a, int b) { return a + b; }
			}

			private ConePadSuite NUnitSuite;

			[BeforeEach]
			public void GivenFixtureWithTestCases() {
				NUnitSuite = new NUnitSuiteBuilder().BuildSuite(typeof(FixtureWithTestCases)); 
			}

			public void theres_one_test_per_case() {
				Verify.That(() => NUnitSuite.TestCount == 2);
			}
		}
	}

	public class NullLogger : IConeLogger
	{
		public void BeginSession()
		{ }

		public void EndSession()
		{ }

		public void Info(string format, params object[] args)
		{ }

		public void Failure(ConeTestFailure failure)
		{ }

		public void Success(Core.IConeTest test)
		{ }

		public void Pending(Core.IConeTest test)
		{ }
	}
}
