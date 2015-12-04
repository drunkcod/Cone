using Cone.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Cone.Runners
{
	public class MSTestSuiteBuilder : ConePadSuiteBuilder
	{
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

		static class MsTestAttributeNames
		{
			public const string ClassInitialize =   "Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute";
			public const string ClassCleanup =      "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute";
			public const string TestInitialize =    "Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute";
			public const string TestCleanup =       "Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute";
			public const string TestMethod =        "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
			public const string Ignore =            "Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute";
			public const string ExpectedException = "Microsoft.VisualStudio.TestTools.UnitTesting.ExpectedExceptionAttribute";
		}

		class MSTestSuite : ConePadSuite
		{
			class MSTestMethodClassifier : MethodClassifier
			{
				readonly bool ignoredFixture;
				readonly object[] ignoredTestAttributes = { new PendingAttribute { NoExecute = true } };

				public MSTestMethodClassifier(Type fixtureType, IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) : base(fixtureSink, testSink) {
					this.ignoredFixture = fixtureType.GetCustomAttributes(true).Any(x => x.GetType().FullName == MsTestAttributeNames.Ignore);
				}

				protected override void ClassifyCore(MethodInfo method) {
					var attributes = method.GetCustomAttributes(true);
					var attributeNames = attributes.ConvertAll(x => x.GetType().FullName);

					if(!ignoredFixture)
						ClassifySupportMethods(method, attributeNames);

					if (attributeNames.Contains(MsTestAttributeNames.TestMethod)) {
						var testAttributes = attributes;
						var ignored = ignoredFixture || attributeNames.IndexOf(MsTestAttributeNames.Ignore) != -1;
						if(ignored)
							testAttributes = ignoredTestAttributes;

						var e = attributeNames.IndexOf(MsTestAttributeNames.ExpectedException);
						if(e == -1)
							Test(method, testAttributes, ExpectedTestResult.None);
						else {
							var expectedException = attributes[e];
							var getExpectedException = expectedException.GetType().GetProperty("ExceptionType");
							var getAlloweDerived = expectedException.GetType().GetProperty("AllowDerivedTypes");
							Test(method, testAttributes, ExpectedTestResult.Exception(
								(Type)getExpectedException.GetValue(expectedException, null), 
								(bool)getAlloweDerived.GetValue(expectedException, null)));
						}
					}
					else Unintresting(method);
				}

				private void ClassifySupportMethods(MethodInfo method, string[] attributeNames)
				{
					foreach (var item in attributeNames)
						switch (item)
						{
							case MsTestAttributeNames.ClassInitialize:
								if (method.ReturnType == typeof (void) && method.IsStatic)
								{
									var parameters = method.GetParameters();
									if (parameters.Length == 1 &&
										parameters.First().ParameterType.FullName == "Microsoft.VisualStudio.TestTools.UnitTesting.TestContext")
										BeforeAll(method);
								}
								break;

							case MsTestAttributeNames.ClassCleanup:
								AfterAll(method);
								break;
							case MsTestAttributeNames.TestInitialize:
								BeforeEach(method);
								break;
							case MsTestAttributeNames.TestCleanup:
								AfterEach(method);
								break;
						}
				}
			}

			public MSTestSuite(ConeFixture fixture) : base(fixture) { }

			protected override IMethodClassifier GetMethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) {
				return new MSTestMethodClassifier(FixtureType, fixtureSink, testSink);
			}
		}

		public MSTestSuiteBuilder(FixtureProvider objectProvider) : base(objectProvider) { }

		public override bool SupportedType(Type type) {
			return IsTestClass(type) && (type.DeclaringType == null || !IsTestClass(type.DeclaringType));
		}

		private static bool IsTestClass(Type type) {
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

		protected override bool TryGetContext(Type nestedType, out IContextDescription context) {
			context = IsTestClass(nestedType) 
				? new MSTestContextDescription(nestedType) 
				: null;
			return context != null;
		}
	}
}
