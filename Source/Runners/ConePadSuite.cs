using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	public abstract class ConeTestMethodSink : IConeTestMethodSink
	{
		readonly ConeTestNamer names;
		readonly RowSuiteLookup<IRowSuite> rowSuites;


		public ConeTestMethodSink(ConeTestNamer names) {
			this.names = names;
			this.rowSuites = new RowSuiteLookup<IRowSuite>(CreateRowSuite);
		}

		public void Test(MethodInfo method) { TestCore(method); }

		public void RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
			GetRowSuite(method).Add(rows);
		}

		public void RowSource(MethodInfo method) {                 
			var rows = ((IEnumerable<IRowTestData>)FixtureInvoke(method))
				.GroupBy(x => x.Method, x => x as IRowData);
			foreach(var item in rows)
				RowTest(item.Key, item);
		}

		protected abstract void TestCore(MethodInfo method);
		protected abstract object FixtureInvoke(MethodInfo method);
		protected abstract IRowSuite CreateRowSuite(MethodInfo method, string context);

		protected ConeMethodThunk CreateMethodThunk(MethodInfo method) {
			return new ConeMethodThunk(method, names);
		}

		IRowSuite GetRowSuite(MethodInfo method) {
			return rowSuites.GetSuite(CreateMethodThunk(method));
        }
	}

	public class ConePadSuite : IConeSuite
    {
        class ConePadTestMethodSink : ConeTestMethodSink
        {
            readonly IConeFixture fixture;
			readonly string context;
			readonly ConePadSuite suite;

            public ConePadTestMethodSink(ConeTestNamer names, IConeFixture fixture, string context, ConePadSuite suite) : base(names) {
                this.fixture = fixture;
				this.context = context;
            	this.suite = suite;
            }

            public Action<string, ConeMethodThunk, object[], object> TestFound;

            protected override void TestCore(MethodInfo method) {
				var thunk = CreateMethodThunk(method);
				TestFound(context, thunk, null, null); 
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
					var itemName = new ConeTestName(Name, thunk.NameFor(item.Parameters));
					NewTest(itemName, thunk, item.Parameters, item.Result);
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
            
        internal void NewTest(ITestName displayName, ConeMethodThunk thunk, object[] args, object result) {
			tests.Add(new ConePadTest(displayName, fixture, thunk.Method, args, result, thunk));
        }

		public void DiscoverTests(ConeTestNamer names) {
			var testSink = new ConePadTestMethodSink(names, fixture, Name, this);
			testSink.TestFound += (context, thunk, args, result) => NewTest(thunk.TestNameFor(context, args), thunk, args, result);
			var setup = new ConeFixtureSetup(GetMethodClassifier(fixture, testSink));
			setup.CollectFixtureMethods(fixture.FixtureType);
		}

        protected virtual IMethodClassifier GetMethodClassifier(
			IConeFixtureMethodSink fixtureSink, 
			IConeTestMethodSink testSink) {
        	return new ConeMethodClassifier(fixtureSink, testSink);
        }

        public void Run(TestSession session) {
			fixture.WithInitialized(
            	x => session.CollectResults(tests.Cast<IConeTest>(), x), 
            	_ => { }, 
            	_ => { });
        }
    }
}
