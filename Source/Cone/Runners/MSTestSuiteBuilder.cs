using Cone.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Runners
{
	public class MSTestSuiteBuilder : ConePadSuiteBuilder
	{
		static class MSTestAttributeNames
		{
			public const string ClassInitialize =   "Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute";
			public const string ClassCleanup =      "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute";
			public const string TestInitialize =    "Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute";
			public const string TestCleanup =       "Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute";
			public const string TestContext =       "Microsoft.VisualStudio.TestTools.UnitTesting.TestContext";
			public const string TestClass =         "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
			public const string TestMethod =        "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
			public const string Ignore =            "Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute";
			public const string ExpectedException = "Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute";
		}

		static readonly string[] NoStrings = new string[0];

		class MSTestFixtureDescription : IFixtureDescription
		{
			private readonly Type type;

			public MSTestFixtureDescription(Type type) {
				this.type = type;
			}

			public IEnumerable<string> Categories => NoStrings;
			public string SuiteName => type.Namespace; 
			public string SuiteType => "TestClass";
			public string TestName => type.Name;
		}

		class MSTestContextDescription : IContextDescription
		{
			readonly Type type;

			public MSTestContextDescription(Type type) {
				this.type = type;
			}

			public string Context => type.Name; 
			public IEnumerable<string> Categories => NoStrings;
		}

		class MSTestSuite : ConePadSuite
		{
			class MSTestMethodClassifier : MethodClassifier
			{
				static readonly object[] NoExecute = { new PendingAttribute { NoExecute = true } };

				readonly bool ignoredFixture;

				public MSTestMethodClassifier(Type fixtureType, IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) : base(fixtureSink, testSink) {
					this.ignoredFixture = fixtureType.GetCustomAttributes(true).Any(x => x.GetType().FullName == MSTestAttributeNames.Ignore);
				}

				protected override void ClassifyCore(MethodInfo method) {
					var attributes = method.GetCustomAttributes(true);
					var attributeNames = attributes.ConvertAll(x => x.GetType().FullName);

					if(!ignoredFixture)
						ClassifySupportMethods(method, attributeNames);

					if(attributeNames.Contains(MSTestAttributeNames.TestMethod)) {
						var testAttributes = attributes;
						var ignored = ignoredFixture || attributeNames.Contains(MSTestAttributeNames.Ignore);
						if(ignored)
							testAttributes = NoExecute;

						var e = attributeNames.IndexOf(MSTestAttributeNames.ExpectedException);
						var expectedResult = e == -1
							? ExpectedTestResult.None
							: GetExpectedExceptionResult(attributes[e]);

						Test(method, testAttributes, expectedResult);
					}
					else Unintresting(method);
				}

				private static ExpectedTestResult GetExpectedExceptionResult(object expectedException) {
					var getExpectedException = expectedException.GetType().GetProperty("ExceptionType");
					var getAlloweDerived = expectedException.GetType().GetProperty("AllowDerivedTypes");
					return ExpectedTestResult.Exception(
						(Type)getExpectedException.GetValue(expectedException, null),
						(bool)getAlloweDerived.GetValue(expectedException, null));
				}

				private void ClassifySupportMethods(MethodInfo method, string[] attributeNames) {
					foreach(var item in attributeNames)
						switch(item) {
							case MSTestAttributeNames.ClassInitialize:
								if(method.ReturnType == typeof (void) && method.IsStatic) {
									var parameters = method.GetParameters();
									if(parameters.Length == 1 &&
										parameters[0].ParameterType.FullName == MSTestAttributeNames.TestContext)
										BeforeAll(method);
								}
								break;

							case MSTestAttributeNames.ClassCleanup: AfterAll(method); break;
							case MSTestAttributeNames.TestInitialize: BeforeEach(method); break;
							case MSTestAttributeNames.TestCleanup: AfterEach(method); break;
						}
				}
			}

			public MSTestSuite(ConeFixture fixture) : base(fixture) { }

			protected override IMethodClassifier GetMethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) {
				return new MSTestMethodClassifier(FixtureType, fixtureSink, testSink);
			}
		}

		public MSTestSuiteBuilder(ITestNamer testNamer, FixtureProvider objectProvider) : base(testNamer, objectProvider) { }

		public override bool SupportedType(Type type) {
			return IsTestClass(type) && (type.DeclaringType == null || !IsTestClass(type.DeclaringType));
		}

		private static bool IsTestClass(Type type) {
			return type.GetCustomAttributes(true)
				.Any(x => x.GetType().FullName == MSTestAttributeNames.TestClass);
		}

		public override IFixtureDescription DescriptionOf(Type fixtureType) {
			return new MSTestFixtureDescription(fixtureType);
		}

		protected override ConePadSuite NewSuite(Type type, IFixtureDescription description) {
			return new MSTestSuite(MakeFixture(type, description.Categories)) {
				Name = description.SuiteName + "." + description.TestName
			};
		}

		protected override bool TryGetContext(Type nestedType, out IContextDescription context) {
			context = IsTestClass(nestedType) 
				? new MSTestContextDescription(nestedType) 
				: null;
			return context != null;
		}
	}
}
