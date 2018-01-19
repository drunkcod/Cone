using System;
using System.Collections.Generic;
using System.Linq;
using Cone.Core;

namespace Cone.Runners
{
	public class ConeSuite : IConeSuite
	{
		class ConePadTestMethodSink : ConeTestMethodSink
		{
			IConeFixture Fixture => suite.Fixture;

			readonly ConeSuite suite;

			public ConePadTestMethodSink(ITestNamer names, ConeSuite suite) : base(names) {
				this.suite = suite;
			}

			public Action<Invokable, ConeTestMethodContext> TestFound;

			protected override void TestCore(Invokable method, ConeTestMethodContext context) =>
				TestFound(method, context);

			protected override object FixtureInvoke(Invokable method) =>
				method.Await(Fixture.GetFixtureInstance(), null);

			protected override IRowSuite CreateRowSuite(Invokable method, string suiteName) =>
				suite.AddRowSuite(names, method, suiteName);
		}

		class ConePadRowSuite : IRowSuite
		{
			readonly ConeSuite parent;
			readonly ITestNamer names;
			readonly Invokable test;

			public ConePadRowSuite(ConeSuite parent, ITestNamer names, Invokable thunk) {
				this.parent = parent;
				this.names = names;
				this.test = thunk;
			}

			public string Name;

			public void Add(IEnumerable<IRowData> rows) {
				rows.ForEach(item =>
					parent.AddTest(NameFor(item), test, 
						new ConeTestMethodContext(
							item.Parameters,
							item.HasResult 
								? ExpectedTestResult.Value(item.Result)
								: ExpectedTestResult.None, 
							ConeTestMethodContext.Null.Categories, ConeTestMethodContext.NoAttributes)));
			}

			ConeTestName NameFor(IRowData item) =>
				new ConeTestName(Name, item.DisplayAs ?? names.NameFor(test, item.Parameters));
		}

		private readonly ConeFixture fixture;

		public IConeFixture Fixture => fixture;
		readonly List<ConeSuite> subsuites = new List<ConeSuite>();
		IEnumerable<string> categories = Enumerable.Empty<string>();

		public readonly List<IConeTest> Tests = new List<IConeTest>();

		public ConeSuite(ConeFixture fixture) {
			this.fixture = fixture;
		}

		public string Name { get; set; }
		public IEnumerable<string> Categories => categories; 
		public IEnumerable<ConeSuite> Subsuites => subsuites;

		public Type FixtureType => Fixture.FixtureType;
		public int TestCount => Tests.Count + Subsuites.Sum(x => x.TestCount);

		public void AddSubSuite(ConeSuite suite) => subsuites.Add(suite);

		public IRowSuite AddRowSuite(ITestNamer names, Invokable test, string suiteName) =>
			new ConePadRowSuite(this, names, test) {
				Name = Name + "." + suiteName
			};

		public void AddCategories(IEnumerable<string> categories) { this.categories = this.Categories.Concat(categories); }
			
		void AddTest(ITestName displayName, Invokable thunk, ConeTestMethodContext context) =>
			Tests.Add(NewTest(displayName, thunk, context));

		IConeTest NewTest(ITestName displayName, Invokable thunk, ConeTestMethodContext context) =>
			new ConeTest(this, displayName, NewTestMethod(thunk, context), context);

		ConeTestMethod NewTestMethod(Invokable method, ConeTestMethodContext context) {
			switch(context.ExpectedResult.ResultType) {
				case ExpectedTestResultType.None: return new ConeTestMethod(method);
				case ExpectedTestResultType.Value: return new ValueResultTestMethod(method, context.ExpectedResult);
				case ExpectedTestResultType.Exception: return new ExpectedExceptionTestMethod(method, context.ExpectedResult);
				default: throw new NotSupportedException();
			}
		}

		public void DiscoverTests(ITestNamer names) {
			var testSink = new ConePadTestMethodSink(names, this);
			testSink.TestFound += (thunk, context) => AddTest(names.TestNameFor(Name, thunk, context.Arguments), thunk, context);
			var setup = new ConeFixtureSetup(GetMethodClassifier(fixture.FixtureMethods, testSink));
			setup.CollectFixtureMethods(Fixture.FixtureType);
		}
		
		protected virtual IMethodClassifier GetMethodClassifier(
			IConeFixtureMethodSink fixtureSink, 
			IConeTestMethodSink testSink) {
			return new ConeMethodClassifier(fixtureSink, testSink);
		}

		public void Run(Action<IEnumerable<IConeTest>, IConeFixture> collectResults) =>
			collectResults(Tests, Fixture);

		public void RunAll(Action<IEnumerable<IConeTest>, IConeFixture> collectResults) {
			Run(collectResults);
			Subsuites.Flatten(x => x.Subsuites).ForEach(x => x.Run(collectResults));
		}
	}
}
