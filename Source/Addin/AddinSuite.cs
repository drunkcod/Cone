using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Cone.Core;
using NUnit.Core;

namespace Cone.Addin
{
    public class AddinSuite : TestSuite, IConeSuite, IFixtureHolder
    {
        readonly Type type;
        readonly TestExecutor testExecutor;
        readonly ConeTestNamer testNamer;
        readonly RowSuiteLookup<ConeRowSuite> rowSuites;
        readonly string suiteType;
        MethodInfo[] afterEachWithResult;
        readonly ConeFixture fixture;

        internal AddinSuite(Type type, string parentSuiteName, string name, string suiteType, ConeTestNamer testNamer) : base(parentSuiteName, name) {
            this.type = type;
            this.suiteType = suiteType;
            this.testNamer = testNamer;
            this.fixture = new ConeFixture(this);
            this.testExecutor = new TestExecutor(this.fixture);
            this.rowSuites = new RowSuiteLookup<ConeRowSuite>((method, suiteSame) => {
                var suite = new ConeRowSuite(new ConeMethodThunk(method, testNamer), this, testExecutor, suiteSame);
                AddWithAttributes(method, suite);
                return suite;
            });

            var pending = type.FirstOrDefault((IPendingAttribute x) => x.IsPending);
            if(pending != null) {
                RunState = RunState.Ignored;
                IgnoreReason = pending.Reason;
            }
        }

        public string Name { get { return TestName.FullName; } }

        MethodInfo[] IFixtureHolder.SetupMethods { get { return setUpMethods; } }
        MethodInfo[] IFixtureHolder.TeardownMethods { get { return tearDownMethods; } }
        MethodInfo[] IFixtureHolder.AfterEachWithResult { get { return afterEachWithResult; } }

        public override Type FixtureType { get { return type; } }

        public override string TestType { get { return suiteType; } }

        public void BindTo(ConeFixtureMethods setup) {
            fixtureSetUpMethods = setup.BeforeAll;
            setUpMethods = setup.BeforeEach;
            tearDownMethods = setup.AfterEach;
            afterEachWithResult = setup.AfterEachWithResult;
            fixtureTearDownMethods = setup.AfterAll;
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
        
        void AddRowTest(MethodInfo method, IEnumerable<IRowData> rows) {
            rowSuites.GetSuite(method, testNamer.NameFor(method)).Add(rows);
        }

        void IConeSuite.AddSubsuite(IConeSuite suite) {
            AddWithAttributes(type, (Test)suite);
        }

        void AddWithAttributes(ICustomAttributeProvider method, Test test) {
            test.ProcessExplicitAttributes(method);
            Add(test);
        }

        void IConeTestMethodSink.Test(MethodInfo method) {
            AddTestMethod(new ConeMethodThunk(method, testNamer));
        }

        void IConeTestMethodSink.RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
            AddRowTest(method, rows);
        }

        void IConeTestMethodSink.RowSource(MethodInfo method) {
            var rows = ((IEnumerable<IRowTestData>)fixture.Invoke(method))
                .GroupBy(x => x.Method, x => x as IRowData);
            foreach(var item in rows)
                AddRowTest(item.Key, item);
        }

        IConeSuite AsSuite() { return (IConeSuite)this; }
    }
}
 