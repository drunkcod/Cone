using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	public class NUnitSuiteBuilder : ConePadSuiteBuilder
	{
		class NUnitFixtureDescription : IFixtureDescription 
		{
			private readonly Type type;

			public NUnitFixtureDescription(Type type) {
				this.type = type;
			}

			public IEnumerable<string> Categories
			{
				get { 
					return type.GetCustomAttributes(true)
						.Select(x => new { Type = x.GetType(), Item = x })
						.Where(x => IsCategoryAttribute(x.Type))
						.Select(x => x.Type.GetProperty("Name").GetValue(x.Item, null).ToString());
				}
			}

			static bool IsCategoryAttribute(Type type) {
				if(type == null)
					return false;
				return type.FullName == "NUnit.Framework.CategoryAttribute" || IsCategoryAttribute(type.BaseType); 
			}

			public string SuiteName => type.Namespace;
			public string SuiteType => "TestFixture";
			public string TestName => type.Name;
		}

		class NUnitSuite : ConeSuite 
		{
			class NUnitMethodClassifier : MethodClassifier
			{
				readonly Type fixtureType;

				public NUnitMethodClassifier(Type fixtureType, IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) : base(fixtureSink, testSink) {
					this.fixtureType = fixtureType;
				}

				protected override void ClassifyCore(Invokable method) {
					var attributes = method.GetCustomAttributes(true);
					if(method.GetParameters().Length > 0) {
						ClassifyParameterized(method, attributes);
						return;
					}

					var attributeNames = attributes.ConvertAll(x => x.GetType().FullName);
					foreach (var item in attributeNames) {
						switch(item) {
							case "NUnit.Framework.SetUpAttribute": BeforeEach(method); break;
							case "NUnit.Framework.TearDownAttribute": AfterEach(method); break;
							case "NUnit.Framework.TestFixtureSetUpAttribute": BeforeAll(method); break;
							case "NUnit.Framework.TestFixtureTearDownAttribute": AfterAll(method); break;
						}
					}

					if(method.ReturnType == typeof(void) && attributeNames.Contains("NUnit.Framework.TestAttribute")) {
						var expectsException = attributes.FirstOrDefault(x => x.GetType().FullName == "NUnit.Framework.ExpectedExceptionAttribute");
						Test(method, new ConeTestMethodContext(expectsException == null ? ExpectedTestResult.None : ExpectedTestResult.Exception((Type)expectsException.GetPropertyValue("ExpectedException"), false), NoCategories, attributes));
					}
					else Unintresting(method);
				}

				static string[] NoCategories = new string[0];

				void ClassifyParameterized(Invokable method, object[] attributes) {
					var testCases = attributes.Where(x => x.GetType().FullName == "NUnit.Framework.TestCaseAttribute").ToList();
					var testSources = attributes.Where(x => x.GetType().FullName == "NUnit.Framework.TestCaseSourceAttribute").ToList();
					if(testCases.Count == 0 && testSources.Count == 0) {
						Unintresting(method);
						return;
					}
					RowTest(method, testCases.Select(x => (IRowData)new NUnitRowDataAdapter(x)).Concat(ReadSources(testSources)));
				}

				IEnumerable<IRowData> ReadSources(IEnumerable<object> testSources) {
					var source = new NUnitTestCaseSource(fixtureType);
					return testSources.SelectMany(source.GetTestCases);
				}

				class NUnitRowDataAdapter : IRowData 
				{
					readonly Type testCaseType;
					readonly object testCase;

					public NUnitRowDataAdapter(object testCase) {
						this.testCase = testCase;
						this.testCaseType = testCase.GetType();
					}

					public bool IsPending => false; 
					public string DisplayAs => (string)GetPropertyValue("TestName");
					public object[] Parameters => (object[])GetPropertyValue("Arguments"); 
					public bool HasResult => true;
					public object Result => GetPropertyValue("Result");

					object GetPropertyValue(string name) {
						return testCaseType.GetProperty(name).GetValue(testCase, null);
					}
				}

				class NUnitTestCaseSource
				{
					readonly Type defaultSource;

					public NUnitTestCaseSource(Type defaultSource) {
						this.defaultSource = defaultSource;
					}

					public IEnumerable<IRowData> GetTestCases(object testCaseSource) {
						var sourceType = (Type)testCaseSource.GetPropertyValue("SourceType") ?? defaultSource;
						var sourceObject = GetSourceObject(sourceType);
						var sourceName = (string)testCaseSource.GetPropertyValue("SourceName");
						var sourceMethod = GetSourceMethod(sourceType, sourceName);

						if(sourceMethod == null || (sourceObject == null && !sourceMethod.IsStatic))
							throw new NotSupportedException("Failed to locate TestCaseSource:" + sourceType.FullName + "." + sourceName);
						foreach(var item in ((IEnumerable)sourceMethod.Invoke(sourceObject, null)))
							yield return new NUnitRowDataAdapter(item);
					}

					private MethodInfo GetSourceMethod(Type sourceType, string sourceName) {
						const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
						var method = sourceType.GetMethod(sourceName, flags);
						if(method != null)
							return method;
						var prop = sourceType.GetProperty(sourceName, flags);
						return (prop != null && prop.CanRead) ? prop.GetGetMethod(true) : null;
					}

					private object GetSourceObject(Type sourceType) {
						var ctor = sourceType.GetConstructor(Type.EmptyTypes);
						return ctor == null ? null : ctor.Invoke(null);
					}
				}
			}

			public NUnitSuite(ConeFixture fixture, string name) : base(fixture, name) { }

			protected override IMethodClassifier GetMethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink)
			{
				return new NUnitMethodClassifier(FixtureType, fixtureSink, testSink);
			}
		}

		public NUnitSuiteBuilder(ITestNamer testNamer, FixtureProvider objectProvider) : base(testNamer, objectProvider) { }

		public override bool SupportedType(Type type) => type
			.GetCustomAttributes(true)
			.Any(x => x.GetType().FullName == "NUnit.Framework.TestFixtureAttribute"); 

		public override IFixtureDescription DescriptionOf(Type fixtureType) =>
			new NUnitFixtureDescription(fixtureType);

		protected override ConeSuite NewSuite(Type type, IFixtureDescription description) =>
			new NUnitSuite(MakeFixture(type, description.Categories), description.SuiteName + "." + description.TestName);
	}
}