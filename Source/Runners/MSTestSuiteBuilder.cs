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

		class MSTestSuite : ConePadSuite
		{
			class MSTestMethodClassifier : MethodClassifier
			{
				readonly Type fixtureType;

				public MSTestMethodClassifier(Type fixtureType, IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink)
					: base(fixtureSink, testSink)
				{
					this.fixtureType = fixtureType;
				}

				protected override void ClassifyCore(MethodInfo method)
				{
					if (method.GetParameters().Length > 0)
					{
						Unintresting(method);
						return;
					}

					var attributes = method.GetCustomAttributes(true);
					var attributeNames = attributes.ConvertAll(x => x.GetType().FullName);

					if (attributeNames.Contains("Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute"))
						BeforeEach(method);

					if (attributeNames.Contains("Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute"))
						AfterEach(method);

					if (attributeNames.Contains("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"))
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

		public override bool SupportedType(Type type)
		{
			return type.GetCustomAttributes(true)
				.Any(x => x.GetType().FullName == "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
		}

		public override IFixtureDescription DescriptionOf(Type fixtureType)
		{
			return new MSTestFixtureDescription(fixtureType);
		}

		protected override ConePadSuite NewSuite(Type type, IFixtureDescription description)
		{
			return new MSTestSuite(MakeFixture(type, description.Categories))
			{
				Name = description.SuiteName + "." + description.TestName
			};
		}
	}
}
