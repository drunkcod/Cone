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

			public string SuiteName {
				get { return type.Namespace; }
			}

			public string SuiteType {
				get { return "TestFixture"; }
			}

			public string TestName {
				get { return type.Name; }
			}
		}

		class NUnitSuite : ConePadSuite 
		{
			class NUnitMethodClassifier : MethodClassifier
			{
				readonly Type fixtureType;

				public NUnitMethodClassifier(Type fixtureType, IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink) : base(fixtureSink, testSink) {
					this.fixtureType = fixtureType;
				}

				protected override void ClassifyCore(MethodInfo method) {
					var attributes = method.GetCustomAttributes(true);
					if(method.GetParameters().Length > 0) {
						ClassifyParameterized(method, attributes);
						return;
					}
					var attributeNames = attributes.Select(x => x.GetType().FullName).ToArray();
					if(attributeNames.Contains("NUnit.Framework.SetUpAttribute")) {
						BeforeEach(method);
					}
					if(attributeNames.Contains("NUnit.Framework.TearDownAttribute")) {
						AfterEach(method);
					}
					if(attributeNames.Contains("NUnit.Framework.TestFixtureSetUpAttribute")) {
						BeforeAll(method);
					}
					if(attributeNames.Contains("NUnit.Framework.TestFixtureTearDownAttribute")) {
						AfterAll(method);
					}

					if(method.ReturnType == typeof(void) && attributeNames.Contains("NUnit.Framework.TestAttribute")) {
						var expectsException = attributes.FirstOrDefault(x => x.GetType().FullName == "NUnit.Framework.ExpectedExceptionAttribute");
						Test(method, expectsException == null ? ExpectedTestResult.None : ExpectedTestResult.Exception((Type)expectsException.GetPropertyValue("ExpectedException")));
					}
					else Unintresting(method);
				}

				void ClassifyParameterized(MethodInfo method, object[] attributes) {
					var testCases = attributes.Where(x => x.GetType().FullName == "NUnit.Framework.TestCaseAttribute").ToList();
					var testSources = attributes.Where(x => x.GetType().FullName == "NUnit.Framework.TestCaseSourceAttribute").ToList();
					if(testCases.Count == 0 && testSources.Count == 0) {
						Unintresting(method);
						return;
					}
					RowTest(method, testCases.Select(x => (IRowData)new NUnitRowDataAdapter(x)).Concat(ReadSources(testSources)));
				}

				IEnumerable<IRowData> ReadSources(IEnumerable<object> testSources)
				{
					foreach(var item in testSources) {
						var source = new NUnitTestCaseSource(fixtureType, item);
						foreach(var testCase in source.GetTestCases())
							yield return testCase;
					}
				}

				class NUnitRowDataAdapter : IRowData 
				{
					readonly Type testCaseType;
					readonly object testCase;

					public NUnitRowDataAdapter(object testCase) {
						this.testCase = testCase;
						this.testCaseType = testCase.GetType();
					}

					public bool IsPending {
						get { return false; }
					}

					public string DisplayAs {
						get { return (string)GetPropertyValue("TestName"); }
					}

					public object[] Parameters {
						get { return (object[])GetPropertyValue("Arguments"); }
					}

					public bool HasResult { get { return true; } }
					public object Result {
						get { return GetPropertyValue("Result"); }
					}

					object GetPropertyValue(string name) {
						return testCaseType.GetProperty(name).GetValue(testCase, null);
					}
				}

				class NUnitTestCaseSource
				{
					readonly Type sourceType;
					readonly object testCaseSource;

					public NUnitTestCaseSource(Type defaultSource, object testCaseSource) {
						this.testCaseSource = testCaseSource;
						this.sourceType = (Type)testCaseSource.GetPropertyValue("SourceType") ?? defaultSource;
					}

					public IEnumerable<IRowData> GetTestCases() {
						var sourceObject = GetSourceObject();
						var sourceName = (string)testCaseSource.GetPropertyValue("SourceName");
						var sourceMethod = GetSourceMethod(sourceName);
						if(sourceMethod == null || (sourceObject == null && !sourceMethod.IsStatic))
							throw new NotSupportedException("Failed to locate TestCaseSource:" + sourceType.FullName + "." + sourceName);
						foreach(var item in ((IEnumerable)sourceMethod.Invoke(sourceObject, null)))
							yield return new NUnitRowDataAdapter(item);
					}

					private MethodInfo GetSourceMethod(string sourceName) {
						const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
						var method = sourceType.GetMethod(sourceName, flags);
						if(method != null)
							return method;
						var prop = sourceType.GetProperty(sourceName, flags);
						return (prop != null && prop.CanRead) ? prop.GetGetMethod(true) : null;
					}

					private object GetSourceObject() {
						var ctor = sourceType.GetConstructor(Type.EmptyTypes);						
						return ctor == null ? null : ctor.Invoke(null);
					}
				}
			}

			public NUnitSuite(ConeFixture fixture) : base(fixture) { }

			protected override IMethodClassifier GetMethodClassifier(IConeFixtureMethodSink fixtureSink, IConeTestMethodSink testSink)
			{
				return new NUnitMethodClassifier(FixtureType, fixtureSink, testSink);
			}
		}

		public NUnitSuiteBuilder(ObjectProvider objectProvider) : base(objectProvider) { }

		public override bool SupportedType(Type type)
		{
			return type.GetCustomAttributes(true)
				.Any(x => x.GetType().FullName == "NUnit.Framework.TestFixtureAttribute"); 
		}

		public override IFixtureDescription DescriptionOf(Type fixtureType)
		{
			return new NUnitFixtureDescription(fixtureType);
		}

		protected override ConePadSuite NewSuite(Type type, IFixtureDescription description) {
			return new NUnitSuite(MakeFixture(type, description.Categories)) { 
				Name = description.SuiteName + "." + description.TestName
			};
		}
	}
}