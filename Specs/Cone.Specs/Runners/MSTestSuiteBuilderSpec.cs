using Cone.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

//mimic MSTest framework attributes
namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
	public class TestClassAttribute : Attribute { }

	public class TestMethodAttribute : Attribute { }

	public class TestInitializeAttribute : Attribute { }

	public class TestCleanupAttribute : Attribute { }
}

namespace Cone.Runners
{
	[Describe(typeof(MSTestSuiteBuilder))]
	public class MSTestSuiteBuilderSpec
	{
		readonly MSTestSuiteBuilder SuiteBuilder = new MSTestSuiteBuilder(new DefaultObjectProvider());
		
		[TestClass]
		class MyMSTestFixture
		{
			public int Calls;
			public int TestCalled;
			public int TestInitializeCalled;
			public int TestCleanupCalled;

			[TestMethod]
			public void a_test() { TestCalled = ++Calls; }

			[TestInitialize]
			public void TestInitialize() { TestInitializeCalled = ++Calls; }

			[TestCleanup]
			public void TestCleanup() { TestCleanupCalled = ++Calls; }


			public void NotATest() { ++Calls; }
		}

		public void supports_types_with_TestClass_attribute() {
			Check.That(() => SuiteBuilder.SupportedType(typeof(MyMSTestFixture)));
		}

		[Context("given description of MyMSTestFixture")]
		public class MSTestSuiteBuilderFixtureDescriptionSpec
		{
			IFixtureDescription Description;

			[BeforeAll]
			public void Given_description_of_MyMSTestFixture() {
				var suiteBuilder = new MSTestSuiteBuilder(new DefaultObjectProvider());
				Description = suiteBuilder.DescriptionOf(typeof(MyMSTestFixture));
			}

			public void suite_type_is_TestClass() {
				Check.That(() => Description.SuiteType == "TestClass");
			}

			public void test_name_is_name_of_fixtur() {
				Check.That(() => Description.TestName == typeof(MyMSTestFixture).Name);
			}

			public void suite_name_is_namespace_of_fixture() {
				Check.That(() => Description.SuiteName == typeof(MyMSTestFixture).Namespace);
			}

		}

		[Context("given a test class")]
		public class MSTestSuiteBuilderSimpleTestClassSpec
		{
			MyMSTestFixture MSTestTestClass;
			ConePadSuite MSTestSuite;

			[BeforeAll]
			public void CreateFixtureInstance() {
				MSTestSuite = new MSTestSuiteBuilder(new LambdaObjectProvider(t => MSTestTestClass = new MyMSTestFixture())).BuildSuite(typeof(MyMSTestFixture));
				new TestSession(new NullLogger()).RunSession(collectResult => collectResult(MSTestSuite));
			}

			public void identifies_test_methods() {
				Check.That(() => MSTestSuite.TestCount == 1);
			}

			public void TestInitialize_called_before_test() {
				Check.That(
					() => MSTestTestClass.TestInitializeCalled > 0,
					() => MSTestTestClass.TestInitializeCalled == MSTestTestClass.TestCalled - 1);
			}

			public void TestCleanup_called_after_test() {
				Check.That(() => MSTestTestClass.TestCleanupCalled == MSTestTestClass.TestCalled + 1);
			}
		}
	}
}
