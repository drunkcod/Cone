using Cone.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


//mimic MSTest framework attributes
namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
	public class TestClassAttribute : Attribute { }

	public class TestMethodAttribute : Attribute { }
}

namespace Cone.Runners
{
	class MSTestSuiteBuilder : ConePadSuiteBuilder
	{
		class MSTestFixtureDescription : IFixtureDescription
		{
			private readonly Type type;

			public MSTestFixtureDescription(Type type) {
				this.type = type;
			}

			public IEnumerable<string> Categories {
				get { return new string[0]; }
			}

			public string SuiteName {
				get { return type.Namespace; }
			}

			public string SuiteType {
				get { return "TestClass"; }
			}

			public string TestName {
				get { return type.Name; }
			}
		}

		class MSTestSuite : ConePadSuite
		{
			class MSTestMethodClassifier : MethodClassifier
			{
				readonly Type fixtureType;

				public MSTestMethodClassifier(Type fixtureType, IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) : base(fixtureSink, testSink) {
					this.fixtureType = fixtureType;
				}

				protected override void ClassifyCore(MethodInfo method)
				{
					if (method.GetParameters().Length > 0) {
						Unintresting(method);
						return;
					}

					var attributes = method.GetCustomAttributes(true);
					if (attributes.Any(x => x.GetType().FullName == "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"))
						Test(method, ExpectedTestResult.None);
					else Unintresting(method);
				}
			}

			public MSTestSuite(ConeFixture fixture) : base(fixture) { }

			protected override IMethodClassifier GetMethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink)
			{
				return new MSTestMethodClassifier(FixtureType, fixtureSink, testSink);
			}
		}

		public MSTestSuiteBuilder(ObjectProvider objectProvider) : base(objectProvider) { }

		public override bool SupportedType(Type type) {
			return type.GetCustomAttributes(true)
				.Any(x => x.GetType().FullName == "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
		}

		public override IFixtureDescription DescriptionOf(Type fixtureType) {
			return new MSTestFixtureDescription(fixtureType);
		}

		protected override ConePadSuite NewSuite(Type type, IFixtureDescription description) {
			return new MSTestSuite(MakeFixture(type, description.Categories)) {
				Name = description.SuiteName + "." + description.TestName
			};
		}
	}

	[Describe(typeof(MSTestSuiteBuilder))]
	public class MSTestSuiteBuilderSpec
	{
		readonly MSTestSuiteBuilder SuiteBuilder = new MSTestSuiteBuilder(new DefaultObjectProvider());
		
		[TestClass]
		class MyMSTestFixture
		{
			public int Calls;
			public int TestCalled;

			[TestMethod]
			public void a_test() { TestCalled = ++Calls; }

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

			public void identifies_all_test_methods() {
				Check.That(() => MSTestSuite.TestCount == 1);
			}
		}
	}
}
