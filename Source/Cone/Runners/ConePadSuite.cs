using System;
using System.Collections.Generic;
using System.Linq;
using Cone.Core;

namespace Cone.Runners
{
	public class ConeTestMethodContext
	{
		public static readonly ConeTestMethodContext Null = new ConeTestMethodContext(ExpectedTestResult.None, new string[0]);

		public readonly object[] Arguments;
		public readonly IReadOnlyCollection<string> Categories;
		public readonly ExpectedTestResult ExpectedResult;

		public ConeTestMethodContext(ExpectedTestResult result, IReadOnlyCollection<string> cats) : this(null, result, cats) { }

		public ConeTestMethodContext(object[] arguments, ExpectedTestResult result, IReadOnlyCollection<string> cats) {
			this.Arguments = arguments;
			this.ExpectedResult = result;
			this.Categories = cats;
		}
	}

	public class ConePadSuite : IConeSuite
	{
		class ConePadTestMethodSink : ConeTestMethodSink
		{
			IConeFixture Fixture => suite.Fixture;

			readonly ConePadSuite suite;

			public ConePadTestMethodSink(ITestNamer names, ConePadSuite suite) : base(names) {
				this.suite = suite;
			}

			public Action<ConeMethodThunk, ConeTestMethodContext> TestFound;

			protected override void TestCore(Invokable method, IEnumerable<object> attributes, ConeTestMethodContext context) {
				var thunk = CreateMethodThunk(method, attributes);
				TestFound(thunk, context);
			}

			protected override object FixtureInvoke(Invokable method) =>
				method.Await(Fixture.GetFixtureInstance(), null);

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
					parent.AddTest(NameFor(item), thunk, 
						new ConeTestMethodContext(
							item.Parameters,
							item.HasResult 
								? ExpectedTestResult.Value(item.Result)
								: ExpectedTestResult.None, 
							ConeTestMethodContext.Null.Categories)));
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
			
		void AddTest(ITestName displayName, ConeMethodThunk thunk, ConeTestMethodContext context) =>
			Tests.Add(NewTest(displayName, thunk, context));

		IConeTest NewTest(ITestName displayName, ConeMethodThunk thunk, ConeTestMethodContext context) =>
			new ConePadTest(this, displayName, NewTestMethod(Fixture, thunk.Method, context), context.Arguments, thunk);

		static ConeTestMethod NewTestMethod(IConeFixture fixture, Invokable method, ConeTestMethodContext context) {
			switch(context.ExpectedResult.ResultType) {
				case ExpectedTestResultType.None: return new ConeTestMethod(fixture, method, context.Categories);
				case ExpectedTestResultType.Value: return new ValueResultTestMethod(fixture, method, context.ExpectedResult, context.Categories);
				case ExpectedTestResultType.Exception: return new ExpectedExceptionTestMethod(fixture, method, context.ExpectedResult, context.Categories);
				default: throw new NotSupportedException();
			}
		}

		public void DiscoverTests(ITestNamer names) {
			var testSink = new ConePadTestMethodSink(names, this);
			testSink.TestFound += (thunk, context) => AddTest(thunk.TestNameFor(Name, context.Arguments), thunk, context);
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
