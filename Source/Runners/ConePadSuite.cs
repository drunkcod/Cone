using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	public enum ExpectedTestResultType 
	{
		None,
		Value,
		Exception
	}

	public struct ExpectedTestResult
	{
		public readonly ExpectedTestResultType ResultType;
		public readonly object ExpectedResult;

		ExpectedTestResult(ExpectedTestResultType resultType, object value)
		{
			this.ResultType = resultType;
			this.ExpectedResult = value;
		}

		public static readonly ExpectedTestResult None = new ExpectedTestResult(ExpectedTestResultType.None, null);

		public static ExpectedTestResult Value(object value) {
			return new ExpectedTestResult(ExpectedTestResultType.Value, value);
		}

		public static ExpectedTestResult Exception(Type exceptionType) {
			return new ExpectedTestResult(ExpectedTestResultType.Exception, exceptionType);
		}
	}

	public class ConePadSuite : IConeSuite
    {
        class ConePadTestMethodSink : ConeTestMethodSink
        {
            readonly IConeFixture fixture;
			readonly ConePadSuite suite;

            public ConePadTestMethodSink(ConeTestNamer names, IConeFixture fixture, ConePadSuite suite) : base(names) {
                this.fixture = fixture;
            	this.suite = suite;
            }

            public Action<ConeMethodThunk, object[], ExpectedTestResult> TestFound;

            protected override void TestCore(MethodInfo method, ExpectedTestResult expectedResult) {
				var thunk = CreateMethodThunk(method);
				TestFound(thunk, null, expectedResult); 
			}

			protected override object FixtureInvoke(MethodInfo method) {
				return method.Invoke(fixture.Fixture, null);
			}

			protected override IRowSuite CreateRowSuite(MethodInfo method, string suiteName) {
				return suite.AddRowSuite(CreateMethodThunk(method), suiteName);
			}
        }

		class ConePadRowSuite : ConePadSuite, IRowSuite
		{
			ConeMethodThunk thunk;

			public ConePadRowSuite(ConePadSuite parent, ConeMethodThunk thunk) : base(parent.fixture) {
				this.thunk = thunk;
				AddCategories(parent.Categories);
			}

			public void Add(IEnumerable<IRowData> rows) {
				foreach (var item in rows) {
					var itemName = new ConeTestName(Name, item.DisplayAs ?? thunk.NameFor(item.Parameters));
					NewTest(itemName, thunk, item.Parameters, 
						item.HasResult 
							? ExpectedTestResult.Value(item.Result)
							: ExpectedTestResult.None);
                }
			}
		}

        readonly ConeFixture fixture;
        readonly List<Lazy<ConePadSuite>> @subsuites = new List<Lazy<ConePadSuite>>();
		readonly List<string> @categories = new List<string>();

		readonly List<ConePadTest> tests = new List<ConePadTest>();

        public ConePadSuite(ConeFixture fixture) {
            this.fixture = fixture;
        }

        public string Name { get; set; }
		public IEnumerable<string> Categories { get { return categories; } } 
		public IEnumerable<ConePadSuite> Subsuites { 
			get { 
				return subsuites.Select(x => x.Value);
			} 
		}

		public object Fixture { get { return fixture.Fixture; } }
		public Type FixtureType { get { return fixture.FixtureType; } }
		public int TestCount { get { return tests.Count + Subsuites.Sum(x => x.TestCount); } }

        public void AddSubSuite(Lazy<ConePadSuite> suite) {
            subsuites.Add(suite);
        }

		public IRowSuite AddRowSuite(ConeMethodThunk thunk, string suiteName) {
			var rows = new ConePadRowSuite(this, thunk) {
				Name = Name + "." + suiteName
			};
			AddSubSuite(new Lazy<ConePadSuite>(() => rows));
			return rows;
		}

        public void AddCategories(IEnumerable<string> categories) { this.categories.AddRange(categories); }
            
        void NewTest(ITestName displayName, ConeMethodThunk thunk, object[] args, ExpectedTestResult result) {
			tests.Add(new ConePadTest(displayName, NewTestMethod(thunk.Method, result), args, thunk));
        }

		ConeTestMethod NewTestMethod(MethodInfo method, ExpectedTestResult result) {
			switch(result.ResultType) {
				case ExpectedTestResultType.None: return new ConeTestMethod(fixture, method);
				case ExpectedTestResultType.Value: return new ValueResultTestMethod(fixture, method, result.ExpectedResult);
				case ExpectedTestResultType.Exception: return new ExpectedExceptionTestMethod(fixture, method, (Type)result.ExpectedResult);
				default: throw new NotSupportedException();
			}
		}

		public void DiscoverTests(ConeTestNamer names) {
			var testSink = new ConePadTestMethodSink(names, fixture, this);
			testSink.TestFound += (thunk, args, result) => NewTest(thunk.TestNameFor(Name, args), thunk, args, result);
			var setup = new ConeFixtureSetup(GetMethodClassifier(fixture.FixtureMethods, testSink));
			setup.CollectFixtureMethods(fixture.FixtureType);
		}

        protected virtual IMethodClassifier GetMethodClassifier(
			IConeFixtureMethodSink fixtureSink, 
			IConeTestMethodSink testSink) {
        	return new ConeMethodClassifier(fixtureSink, testSink);
        }

        public void Run(Action<IEnumerable<IConeTest>, IConeFixture> collectResults) {
			fixture.WithInitialized(
            	x => collectResults(tests.Cast<IConeTest>(), x), 
            	_ => { }, 
            	_ => { });
        }
    }
}
