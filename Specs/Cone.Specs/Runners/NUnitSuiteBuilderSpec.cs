using System;
using System.Linq;
using NUnit.Framework;

namespace NUnit.Framework
{
	public class TestFixtureAttribute : Attribute { }

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
		{ }

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
	}
}
