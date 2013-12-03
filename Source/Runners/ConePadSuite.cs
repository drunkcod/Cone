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
            IConeFixture Fixture { get { return suite.fixture; } }

			readonly ConePadSuite suite;

            public ConePadTestMethodSink(ConeTestNamer names, ConePadSuite suite) : base(names) {
            	this.suite = suite;
            }

            public Action<ConeMethodThunk, object[], ExpectedTestResult> TestFound;

            protected override void TestCore(MethodInfo method, ExpectedTestResult expectedResult) {
				var thunk = CreateMethodThunk(method);
				TestFound(thunk, null, expectedResult); 
			}

			protected override object FixtureInvoke(MethodInfo method) {
				return Fixture.Invoke(method);
			}

			protected override IRowSuite CreateRowSuite(MethodInfo method, string suiteName) {
				return suite.AddRowSuite(CreateMethodThunk(method), suiteName);
			}
        }

		class ConePadRowSuite : IRowSuite
		{
			readonly ConePadSuite parent;
			readonly ConeMethodThunk thunk;
			internal readonly List<IConeTest> tests = new List<IConeTest>();

			public ConePadRowSuite(ConePadSuite parent, ConeMethodThunk thunk) {
				this.parent = parent;
				this.thunk = thunk;
			}

			public string Name;

			public void Add(IEnumerable<IRowData> rows) {
				tests.AddRange(rows.Select(item =>
					parent.NewTest(NameFor(item), thunk, item.Parameters, 
						item.HasResult 
							? ExpectedTestResult.Value(item.Result)
							: ExpectedTestResult.None)));
			}

			ConeTestName NameFor(IRowData item) {
				return new ConeTestName(Name, item.DisplayAs ?? thunk.NameFor(item.Parameters));
			}
		}

        readonly ConeFixture fixture;
        readonly List<ConePadSuite> subsuites = new List<ConePadSuite>();
		readonly List<ConePadRowSuite>  rowSuites = new List<ConePadRowSuite>();
		readonly List<string> categories = new List<string>();

		readonly List<IConeTest> tests = new List<IConeTest>();

        public ConePadSuite(ConeFixture fixture) {
            this.fixture = fixture;
        }

        public string Name { get; set; }
		public IEnumerable<string> Categories { get { return categories; } } 
		public IEnumerable<ConePadSuite> Subsuites { 
			get { return subsuites; } 
		}

		public Type FixtureType { get { return fixture.FixtureType; } }
		public int TestCount { get { return tests.Count + Subsuites.Sum(x => x.TestCount) + rowSuites.Sum(x => x.tests.Count); } }

        public void AddSubSuite(ConePadSuite suite) {
            subsuites.Add(suite);
        }

		public IRowSuite AddRowSuite(ConeMethodThunk thunk, string suiteName) {
			var rows = new ConePadRowSuite(this, thunk) {
				Name = Name + "." + suiteName
			};
			rowSuites.Add(rows);
			return rows;
		}

        public void AddCategories(IEnumerable<string> categories) { this.categories.AddRange(categories); }
            
        void AddTest(ITestName displayName, ConeMethodThunk thunk, object[] args, ExpectedTestResult result) {
			tests.Add(NewTest(displayName, thunk, args, result));
        }

		IConeTest NewTest(ITestName displayName, ConeMethodThunk thunk, object[] args, ExpectedTestResult result) {
			return new ConePadTest(displayName, NewTestMethod(fixture, thunk.Method, result), args, thunk);
		}

		static ConeTestMethod NewTestMethod(IConeFixture fixture, MethodInfo method, ExpectedTestResult result) {
			switch(result.ResultType) {
				case ExpectedTestResultType.None: return new ConeTestMethod(fixture, method);
				case ExpectedTestResultType.Value: return new ValueResultTestMethod(fixture, method, result.ExpectedResult);
				case ExpectedTestResultType.Exception: return new ExpectedExceptionTestMethod(fixture, method, (Type)result.ExpectedResult);
				default: throw new NotSupportedException();
			}
		}

		public void DiscoverTests(ConeTestNamer names) {
			var testSink = new ConePadTestMethodSink(names, this);
			testSink.TestFound += (thunk, args, result) => AddTest(thunk.TestNameFor(Name, args), thunk, args, result);
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
            	x => collectResults(tests.Concat(rowSuites.SelectMany(row => row.tests)), x), 
            	_ => { }, 
            	_ => { });
        }
    }
}
