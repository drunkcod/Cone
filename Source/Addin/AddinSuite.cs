using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;
using Cone.Runners;
using NUnit.Core;

namespace Cone.Addin
{
    public class AddinSuite : TestSuite, IConeSuite
    {
        class AddinTestMethodSink : ConeTestMethodSink
        {
            readonly AddinSuite suite;

            public AddinTestMethodSink(AddinSuite suite, ConeTestNamer testNamer) : base(testNamer) {
                this.suite = suite;
            }

            protected override void TestCore(MethodInfo method, ExpectedTestResult expectedResult) {
                suite.AddTestMethod(CreateMethodThunk(method));
            }

			protected override object FixtureInvoke(MethodInfo method) {
				return suite.fixture.Invoke(method);
			}

			protected override IRowSuite CreateRowSuite(MethodInfo method, string context) {
				return suite.AddRowSuite(CreateMethodThunk(method), context);
			}
        }

		readonly TestExecutor testExecutor;
        readonly string suiteType;
        readonly ConeFixture fixture;

        static EventHandler EnterVerifyContext = (s, e) => Verify.Context = ((IConeFixture)s).FixtureType;

        internal AddinSuite(Type type, IFixtureDescription description) : base(description.SuiteName, description.TestName) {
            this.suiteType = description.SuiteType;
            this.fixture = new ConeFixture(type, description.Categories);
            this.fixture.Before += EnterVerifyContext;          
            this.testExecutor = new TestExecutor(this.fixture);

            var pending = type.AsConeAttributeProvider().FirstOrDefault((IPendingAttribute x) => x.IsPending);
            if(pending != null) {
                RunState = RunState.Ignored;
                IgnoreReason = pending.Reason;
            }
        }

        public string Name { get { return TestName.FullName; } }
		IEnumerable<string> IConeEntity.Categories { get { return Categories.Cast<string>(); } }

        public override Type FixtureType { get { return fixture.FixtureType; } }

        public override object Fixture {
            get { return fixture.Fixture; }
            set { throw new InvalidOperationException(); }
        }
        public override string TestType { get { return suiteType; } }

        public override TestResult Run(EventListener listener, ITestFilter filter) {
            listener.SuiteStarted(TestName);
            var result = new TestResult(this);
            try {
                ITestResult resultAdapter = new NUnitTestResultAdapter(result);
                fixture.WithInitialized(_ => {
                    foreach(var item in Tests.Cast<Test>().Where(filter.Pass).Select(x => x.Run(listener, filter)))
 						result.AddResult(item);
					}, ex => {
                        resultAdapter.BeforeFailure(ex);
                        foreach(Test item in Tests) {
                            var failure = new TestResult(item);
                            listener.TestStarted(item.TestName);
                            failure.Error(ex, FailureSite.SetUp);
                            listener.TestFinished(failure);
                            result.AddResult(failure);
                    }                     
                }, resultAdapter.AfterFailure);
            } finally {
                listener.SuiteFinished(result);
            }
            return result;
        }

        public void AddCategories(IEnumerable<string> categories) {
            foreach(var item in categories)
                Categories.Add(item);
        }

        void AddTestMethod(ConeMethodThunk thunk) { 
            AddWithAttributes(thunk, new AddinTestMethod(thunk, this, testExecutor, thunk.NameFor(null))); 
        }

        internal void AddSubSuite(AddinSuite suite) {
            AddWithAttributes(FixtureType.AsConeAttributeProvider(), (Test)suite);
        }

		public IRowSuite AddRowSuite(ConeMethodThunk thunk, string suiteName) {
			var newSuite = new AddinRowSuite(thunk, this, testExecutor, suiteName);
			AddWithAttributes(thunk, newSuite);
			return newSuite;
		}
        
        void AddWithAttributes(IConeAttributeProvider method, Test test) {
            test.ProcessExplicitAttributes(method);
            Add(test);
        }

        public void WithTestMethodSink(ConeTestNamer testNamer, Action<IConeTestMethodSink> action) {
            action(new AddinTestMethodSink(this, testNamer));
        }

        public void WithFixtureMethodSink(Action<IConeFixtureMethodSink> action) {
            action(fixture.FixtureMethods);
        }

		public void DiscoverTests(ConeTestNamer names) {
			WithTestMethodSink(names, testSink =>
			WithFixtureMethodSink(fixtureSink => {
				var setup = new ConeFixtureSetup(new ConeMethodClassifier(fixtureSink, testSink));
				setup.CollectFixtureMethods(fixture.FixtureType);
			}));
		}
    }
}
 