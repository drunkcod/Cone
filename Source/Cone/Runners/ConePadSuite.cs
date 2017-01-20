using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	public class ConePadSuite : IConeSuite
	{
		class ConePadTestMethodSink : ConeTestMethodSink
		{
			IConeFixture Fixture => suite.Fixture;

			readonly ConePadSuite suite;

			public ConePadTestMethodSink(ITestNamer names, ConePadSuite suite) : base(names) {
				this.suite = suite;
			}

			public Action<ConeMethodThunk, object[], ExpectedTestResult> TestFound;

			protected override void TestCore(MethodInfo method, IEnumerable<object> attributes , ExpectedTestResult expectedResult) {
				var thunk = CreateMethodThunk(method, attributes);
				TestFound(thunk, null, expectedResult); 
			}

			protected override object FixtureInvoke(MethodInfo method) {
				return Fixture.Invoke(method);
			}

			protected override IRowSuite CreateRowSuite(ConeMethodThunk method, string suiteName) {
				return suite.AddRowSuite(method, suiteName);
			}
		}

		class ConePadRowSuite : IRowSuite
		{
			readonly ConePadSuite parent;
			readonly ConeMethodThunk thunk;

			public ConePadRowSuite(ConePadSuite parent, ConeMethodThunk thunk) {
				this.parent = parent;
				this.thunk = thunk;
			}

			public string Name;

			public void Add(IEnumerable<IRowData> rows) {
				rows.ForEach(item =>
					parent.AddTest(NameFor(item), thunk, item.Parameters, 
						item.HasResult 
							? ExpectedTestResult.Value(item.Result)
							: ExpectedTestResult.None));
			}

			ConeTestName NameFor(IRowData item) {
				return new ConeTestName(Name, item.DisplayAs ?? thunk.NameFor(item.Parameters));
			}
		}

		private readonly ConeFixture fixture;

		public IConeFixture Fixture => fixture;
		readonly List<ConePadSuite> subsuites = new List<ConePadSuite>();
		IEnumerable<string> categories = Enumerable.Empty<string>();

		public readonly List<IConeTest> Tests = new List<IConeTest>();

		public ConePadSuite(ConeFixture fixture) {
			this.fixture = fixture;
		}

		public string Name { get; set; }
		public IEnumerable<string> Categories => categories; 
		public IEnumerable<ConePadSuite> Subsuites => subsuites;

		public Type FixtureType => Fixture.FixtureType;
		public int TestCount => Tests.Count + Subsuites.Sum(x => x.TestCount);

		public void AddSubSuite(ConePadSuite suite) {
			subsuites.Add(suite);
		}

		public IRowSuite AddRowSuite(ConeMethodThunk thunk, string suiteName) {
			return new ConePadRowSuite(this, thunk) {
				Name = Name + "." + suiteName
			};
		}

		public void AddCategories(IEnumerable<string> categories) { this.categories = this.Categories.Concat(categories); }
			
		void AddTest(ITestName displayName, ConeMethodThunk thunk, object[] args, ExpectedTestResult result) {
			Tests.Add(NewTest(displayName, thunk, args, result));
		}

		IConeTest NewTest(ITestName displayName, ConeMethodThunk thunk, object[] args, ExpectedTestResult result) {
			return new ConePadTest(this, displayName, NewTestMethod(Fixture, thunk.Method, result), args, thunk);
		}

		static ConeTestMethod NewTestMethod(IConeFixture fixture, MethodInfo method, ExpectedTestResult result) {
			switch(result.ResultType) {
				case ExpectedTestResultType.None: return new ConeTestMethod(fixture, method);
				case ExpectedTestResultType.Value: return new ValueResultTestMethod(fixture, method, result);
				case ExpectedTestResultType.Exception: return new ExpectedExceptionTestMethod(fixture, method, result);
				default: throw new NotSupportedException();
			}
		}

		public void DiscoverTests(ITestNamer names) {
			var testSink = new ConePadTestMethodSink(names, this);
			testSink.TestFound += (thunk, args, result) => AddTest(thunk.TestNameFor(Name, args), thunk, args, result);
			var setup = new ConeFixtureSetup(GetMethodClassifier(fixture.FixtureMethods, testSink));
			setup.CollectFixtureMethods(Fixture.FixtureType);
		}

		protected virtual IMethodClassifier GetMethodClassifier(
			IConeFixtureMethodSink fixtureSink, 
			IConeTestMethodSink testSink) {
			return new ConeMethodClassifier(fixtureSink, testSink);
		}

		public void Run(Action<IEnumerable<IConeTest>, IConeFixture> collectResults) {
			collectResults(Tests, Fixture);
		}

		public void RunAll(Action<IEnumerable<IConeTest>, IConeFixture> collectResults) {
			Run(collectResults);
			Subsuites.Flatten(x => x.Subsuites).ForEach(x => x.Run(collectResults));
		}
	}
}
