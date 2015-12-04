using Cone.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

//mimic MSTest framework attributes
namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
	public class TestClassAttribute : Attribute { }

	public class TestMethodAttribute : Attribute { }

	public class TestInitializeAttribute : Attribute { }

	public class TestCleanupAttribute : Attribute { }

	public class ClassInitializeAttribute : Attribute { }

	public class ClassCleanupAttribute : Attribute { }

	public class IgnoreAttribute : Attribute { }

	public class TestContext { }

	public class ExpectedExceptionAttribute : Attribute 
	{
		readonly Type exceptionType;

		public ExpectedExceptionAttribute(Type exceptionType) {
			this.exceptionType = exceptionType;
		}

		public bool AllowDerivedTypes { get; set; }
		public Type ExceptionType { get { return exceptionType; } }

	}
}

namespace Cone.Runners
{
	[Describe(typeof(MSTestSuiteBuilder))]
	public class MSTestSuiteBuilderSpec
	{
		readonly MSTestSuiteBuilder SuiteBuilder = new MSTestSuiteBuilder(new DefaultFixtureProvider());
		
		[TestClass]
		class MyMSTestFixture
		{
			public static int Calls;
			public static int ClassInitializeCalled;
			public static int ClassCleanupCalled;
			public int TestCalled;
			public int TestInitializeCalled;
			public int TestCleanupCalled;
			public int IgnoreCalled;

			[TestClass]
			public class NestedMSTestFixture { }

			[ClassInitialize]
			public static void ClassInitialize(TestContext _) { ClassInitializeCalled = ++Calls; }

			[ClassCleanup]
			public static void ClassCleanup() { ClassCleanupCalled = ++Calls; }

			[TestMethod]
			public void a_test() { TestCalled = ++Calls; }

			[TestMethod,Ignore]
			public void a_ignored_test() { ++IgnoreCalled; }

			[TestInitialize]
			public void TestInitialize() { TestInitializeCalled = ++Calls; }

			[TestCleanup]
			public void TestCleanup() { TestCleanupCalled = ++Calls; }

			public void NotATest() { ++Calls; }
		}

		public void supports_types_with_TestClass_attribute() {
			Check.That(() => SuiteBuilder.SupportedType(typeof(MyMSTestFixture)));
		}

		public void nested_fixtures_are_added_as_children()
		{
			var suite = SuiteBuilder.BuildSuite(typeof(MyMSTestFixture));
			Check.That(() => suite.Subsuites.Count() == 1);
		}

		public void nested_fixtures_are_not_supported_at_toplevel()
		{
			Check.That(() => SuiteBuilder.SupportedType(typeof(MyMSTestFixture.NestedMSTestFixture)) == false);
		}

		[Context("given description of MyMSTestFixture")]
		public class MSTestSuiteBuilderFixtureDescriptionSpec
		{
			IFixtureDescription Description;

			[BeforeAll]
			public void Given_description_of_MyMSTestFixture() {
				var suiteBuilder = new MSTestSuiteBuilder(new DefaultFixtureProvider());
				Description = suiteBuilder.DescriptionOf(typeof(MyMSTestFixture));
			}

			public void suite_type_is_TestClass() {
				Check.That(() => Description.SuiteType == "TestClass");
			}

			public void test_name_is_name_of_fixture() {
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
				Check.That(() => MSTestSuite.TestCount == 2);
			}

			public void TestInitialize_called_before_test() {
				Check.That(
					() => MSTestTestClass.TestInitializeCalled > 0,
					() => MSTestTestClass.TestInitializeCalled == MSTestTestClass.TestCalled - 1);
			}

			public void TestCleanup_called_after_test() {
				Check.That(() => MSTestTestClass.TestCleanupCalled == MSTestTestClass.TestCalled + 1);
			}

			public void ClassInitialize_called_once_before_all() {
				Check.That(() => MyMSTestFixture.ClassInitializeCalled == 1);
			}

			public void ClassCleanup_called_after_all() {
				Check.That(() => MyMSTestFixture.ClassCleanupCalled == MyMSTestFixture.Calls);
			}

			public void ignores_Ignored_tests() {
				Check.That(() => MSTestTestClass.IgnoreCalled == 0);
			}
		}

		[Context("given expected exceptions")]
		public class MSTestsuiteBuilderExpectedExceptionsSepc
		{
			[TestClass]
			class ExpectedExceptions
			{
				[TestMethod,ExpectedException(typeof(InvalidOperationException))]
				public void invalid_operation() {
					throw new InvalidOperationException();
				}

				[TestMethod,ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
				public void allow_derived_types() {
					throw new InvalidOperationException();
				}

			}

			class RecordingTestSession : ISessionLogger, ISuiteLogger, ITestLogger
			{
				IConeTest currentTest;

				public void WriteInfo(Action<ISessionWriter> output) {}

				public void BeginSession() {}

				public ISuiteLogger BeginSuite(IConeSuite suite) { return this; }

				public void EndSession() { }

				public readonly List<string> Passed = new List<string>();

				public ITestLogger BeginTest(IConeTest test) { 
					currentTest = test;
					return this; 
				}

				public void EndSuite() { }

				public void BeginTest() { }

				public void Failure(ConeTestFailure failure) { }

				public void Success() { Passed.Add(currentTest.TestName.Name); }

				public void Pending(string reason) { }

				public void Skipped() { }

				public void EndTest() { currentTest = null; }
			}

			ConePadSuite MSTestSuite;
			RecordingTestSession TestReport;

			[BeforeAll]
			public void CreateFixtureInstance() {
				MSTestSuite = new MSTestSuiteBuilder(new LambdaObjectProvider(t => new ExpectedExceptions()))
					.BuildSuite(typeof(ExpectedExceptions));
				TestReport = new RecordingTestSession();
				new TestSession(TestReport).RunSession(collectResult => collectResult(MSTestSuite));
			}

			public void identifies_test_methods() {
				Check.That(() => MSTestSuite.TestCount == 2);
			}

			public void test_raising_expected_exception_passes() {
				Check.That(() => TestReport.Passed.Contains("invalid operation"));
			}

			public void allow_derived_types() {
				Check.That(() => TestReport.Passed.Contains("allow derived types"));
			}
		}
	}
}
