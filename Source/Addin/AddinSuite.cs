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
        readonly ConeTestNamer testNamer;
        readonly string suiteType;
        readonly ConeFixture fixture;

        static EventHandler SetVerifyContext = (s, e) => Verify.Context = ((ConeFixture)s).FixtureType;

        internal AddinSuite(Type type, IFixtureDescription description, ConeTestNamer testNamer) : base(description.SuiteName, description.TestName) {
            this.suiteType = description.SuiteType;
            this.testNamer = testNamer;
            this.fixture = new ConeFixture(type);
            this.fixture.Before += SetVerifyContext;          
            this.testExecutor = new TestExecutor(this.fixture);

            var pending = type.FirstOrDefault((IPendingAttribute x) => x.IsPending);
            if(pending != null) {
                RunState = RunState.Ignored;
                IgnoreReason = pending.Reason;
            }
        }

        ConeRowSuite CreateSuite(MethodInfo method, string suiteName) {
            var suite = new ConeRowSuite(new ConeMethodThunk(method, testNamer), this, testExecutor, suiteName);
            AddWithAttributes(method, suite);
            return suite;
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
                });
            } finally {
                listener.SuiteFinished(result);
            }
            return result;
        }

        string NameFor(MethodInfo method) {
            return testNamer.NameFor(method);
        }

        public void AddCategories(IEnumerable<string> categories) {
            foreach(var item in categories)
                Categories.Add(item);
        }

        void AddTestMethod(ConeMethodThunk thunk) { 
            AddWithAttributes(thunk, new ConeTestMethod(thunk, this, testExecutor, thunk.NameFor(null))); 
        }
        
        void IConeSuite.AddSubsuite(IConeSuite suite) {
            AddWithAttributes(FixtureType, (Test)suite);
        }

        void AddWithAttributes(ICustomAttributeProvider method, Test test) {
            test.ProcessExplicitAttributes(method);
            Add(test);
        }

        class AddinTestMethodSink : IConeTestMethodSink
        {
            readonly AddinSuite suite;
            readonly RowSuiteLookup<ConeRowSuite> rowSuites;

            public AddinTestMethodSink(AddinSuite suite) {
                this.suite = suite;
                this.rowSuites = new RowSuiteLookup<ConeRowSuite>(suite.CreateSuite);
            }

            void IConeTestMethodSink.Test(MethodInfo method) {
                suite.AddTestMethod(new ConeMethodThunk(method, suite.testNamer));
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

            void AddRowTest(MethodInfo method, IEnumerable<IRowData> rows) {
                rowSuites.GetSuite(method, suite.testNamer.NameFor(method)).Add(rows);
            }
        }

        public void WithTestMethodSink(Action<IConeTestMethodSink> action) {
            action(new AddinTestMethodSink(this));
        }

        public void WithFixtureMethodSink(Action<IConeFixtureMethodSink> action) {
            action(fixture);
        }

        IConeSuite AsSuite() { return (IConeSuite)this; }
    }
}
 