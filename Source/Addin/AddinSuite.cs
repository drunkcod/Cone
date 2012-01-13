using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;
using NUnit.Core;

namespace Cone.Addin
{
    public class AddinSuite : TestSuite, IConeSuite
    {
        readonly TestExecutor testExecutor;
        readonly string suiteType;
        readonly ConeFixture fixture;

        static EventHandler SetVerifyContext = (s, e) => Verify.Context = ((IConeFixture)s).FixtureType;

        internal AddinSuite(Type type, IFixtureDescription description) : base(description.SuiteName, description.TestName) {
            this.suiteType = description.SuiteType;
            this.fixture = new ConeFixture(type);
            this.fixture.Before += SetVerifyContext;          
            this.testExecutor = new TestExecutor(this.fixture);

            var pending = type.AsConeAttributeProvider().FirstOrDefault((IPendingAttribute x) => x.IsPending);
            if(pending != null) {
                RunState = RunState.Ignored;
                IgnoreReason = pending.Reason;
            }
        }

        public string Name { get { return TestName.FullName; } }

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
                fixture.WithInitialized(new NUnitTestResultAdapter(result), () => {
                    foreach(Test test in Tests)
                        if(filter.Pass(test))
                            result.AddResult(test.Run(listener, filter));
                }, ex => {
                    foreach(Test item in Tests) {
                        var failure = new TestResult(item);
                        listener.TestStarted(item.TestName);
                        failure.Error(ex, FailureSite.SetUp);
                        listener.TestFinished(failure);
                        result.AddResult(failure);
                    }
                        
                });
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
            AddWithAttributes(thunk, new ConeTestMethod(thunk, this, testExecutor, thunk.NameFor(null))); 
        }

        internal void AddSubSuite(AddinSuite suite) {
            AddWithAttributes(FixtureType.AsConeAttributeProvider(), (Test)suite);
        }
        
        void AddWithAttributes(IConeAttributeProvider method, Test test) {
            test.ProcessExplicitAttributes(method);
            Add(test);
        }

        class AddinTestMethodSink : IConeTestMethodSink
        {
            readonly AddinSuite suite;
            readonly ConeTestNamer testNamer;
            readonly RowSuiteLookup<ConeRowSuite> rowSuites;

            public AddinTestMethodSink(AddinSuite suite, ConeTestNamer testNamer) {
                this.suite = suite;
                this.testNamer = testNamer;
                this.rowSuites = new RowSuiteLookup<ConeRowSuite>(CreateSuite);
            }

            void IConeTestMethodSink.Test(MethodInfo method) {
                suite.AddTestMethod(CreateMethodThunk(method));
            }

            void IConeTestMethodSink.RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
                AddRowTest(method, rows);
            }

            void IConeTestMethodSink.RowSource(MethodInfo method) {
                var rows = ((IEnumerable<IRowTestData>)suite.fixture.Invoke(method))
                    .GroupBy(x => x.Method, x => x as IRowData);
                foreach(var item in rows)
                    AddRowTest(item.Key, item);
            }

            ConeTestNamer TestNamer { get { return testNamer; } }

            ConeMethodThunk CreateMethodThunk(MethodInfo method) {
                return new ConeMethodThunk(method, TestNamer);
            }

            void AddRowTest(MethodInfo method, IEnumerable<IRowData> rows) {
                rowSuites.GetSuite(method, TestNamer.NameFor(method)).Add(rows);
            }

            ConeRowSuite CreateSuite(MethodInfo method, string suiteName) {
                var newSuite = new ConeRowSuite(CreateMethodThunk(method), suite, suite.testExecutor, suiteName);
                suite.AddWithAttributes(method.AsConeAttributeProvider(), newSuite);
                return newSuite;
            }
        }

        public void WithTestMethodSink(ConeTestNamer testNamer, Action<IConeTestMethodSink> action) {
            action(new AddinTestMethodSink(this, testNamer));
        }

        public void WithFixtureMethodSink(Action<IConeFixtureMethodSink> action) {
            action(fixture);
        }

        IConeSuite AsSuite() { return (IConeSuite)this; }
    }
}
 