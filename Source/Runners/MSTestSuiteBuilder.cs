using Cone.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Runners
{
	public class MSTestSuiteBuilder : ConePadSuiteBuilder
	{
		class MSTestFixtureDescription : IFixtureDescription
		{
			private readonly Type type;

			public MSTestFixtureDescription(Type type)
			{
				this.type = type;
			}

			public IEnumerable<string> Categories
			{
				get { return new string[0]; }
			}

			public string SuiteName
			{
				get { return type.Namespace; }
			}

			public string SuiteType
			{
				get { return "TestClass"; }
			}

			public string TestName
			{
				get { return type.Name; }
			}
		}

		class MSTestContextDescription : IContextDescription
		{
			readonly Type type;

			public MSTestContextDescription(Type type) {
				this.type = type;
			}

			public string Context {
				get { return type.Name; }
			}

			public IEnumerable<string> Categories {
				get { return new string[0]; }
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

				protected override void ClassifyCore(MethodInfo method) {
					var attributes = method.GetCustomAttributes(true);
					var attributeNames = attributes.ConvertAll(x => x.GetType().FullName);

					foreach(var item in attributeNames)
						switch (item) {
							case "Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute":
								if (method.ReturnType == typeof(void) && method.IsStatic) {
									var parameters = method.GetParameters();
									if(parameters.Length == 1 && parameters.First().ParameterType.FullName == "Microsoft.VisualStudio.TestTools.UnitTesting.TestContext")
										BeforeAll(method);
								}
								break;
							
							case "Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute":
								AfterAll(method);break;
							case "Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute":
								BeforeEach(method); break;
							case "Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute":
								AfterEach(method); break;
						}

					if (attributeNames.Contains("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"))
						Test(method, ExpectedTestResult.None);
					else Unintresting(method);
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

		protected override bool TryGetContext(Type nestedType, out IContextDescription context)
		{
			if(IsTestClass(nestedType)) {
				context = new MSTestContextDescription(nestedType);
			} else context = null;
			return context != null;
		}
	}
}
