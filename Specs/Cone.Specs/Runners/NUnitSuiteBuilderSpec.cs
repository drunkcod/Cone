using System;
using System.Linq;
using NUnit.Framework;

namespace NUnit.Framework
{
	public class TestFixtureAttribute : Attribute { }

	public class SetUpAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CategoryAttribute : Attribute 
	{
		readonly string @name;

		public CategoryAttribute(string name) {
			this.@name = name;
		}

 		public string Name { get { return @name; } }
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
			public bool SetUpCalled;

			[SetUp]
			public void SetUp() { SetUpCalled = true; }

			public void a_test(){}
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
			private ConePadSuite NUnitFixture;

			[BeforeEach]
			public void GivenFixtureInstance() {
				NUnitFixture = new NUnitSuiteBuilder().BuildSuite(typeof(MyNUnitFixture)); 
			}

			public void SetUp_is_called_on_Before() {
				var fixture = (MyNUnitFixture)NUnitFixture.Fixture;
				NUnitFixture.Run(new TestSession(new NullLogger()));
				Verify.That(() => fixture.SetUpCalled == true);
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
