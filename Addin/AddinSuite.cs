using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Core;
using NUnit.Framework;

namespace Cone.Addin
{
    public class AddinSuite : TestSuite, IConeSuite, IFixtureHolder
    {
        readonly Type type;
        readonly TestExecutor testExecutor;
        readonly ConeTestNamer testNamer;
        readonly Dictionary<string,ConeRowSuite> rowSuites = new Dictionary<string,ConeRowSuite>();
        readonly string suiteType;
        MethodInfo[] afterEachWithResult;
        readonly ConeFixture fixtureProvider;

        internal AddinSuite(Type type, string parentSuiteName, string name, string suiteType, ConeTestNamer testNamer) : base(parentSuiteName, name) {
            this.type = type;
            this.suiteType = suiteType;
            this.testNamer = testNamer;
            this.fixtureProvider = new ConeFixture(this);
            this.testExecutor = new TestExecutor(this.fixtureProvider);
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
            
            CreateDynamicRowTests(setup.RowSource);
        }

        void CreateDynamicRowTests(MethodInfo[] rowSources) {
            if(rowSources == null || rowSources.Length == 0)
                return;
            var rows = new Dictionary<MethodInfo, List<IRowData>>();
            var fixture = fixtureProvider.Fixture;
            foreach(var item in rowSources) {
                foreach(IRowTestData row in (IEnumerable<IRowTestData>)item.Invoke(fixture, null)) {
                    List<IRowData> parameters;
                    if(!rows.TryGetValue(row.Method, out parameters))
                        rows[row.Method] = parameters = new List<IRowData>();
                    parameters.Add(row);                 
                }
            }
            IConeSuite suite = this;
            foreach(var item in rows)
                suite.AddRowTest(NameFor(item.Key), item.Key, item.Value);
        }

        string NameFor(MethodInfo method) {
            return testNamer.NameFor(method);
        }

        public void AddCategories(string categories) {
            if(!string.IsNullOrEmpty(categories))
                foreach(var category in categories.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    Categories.Add(category.Trim());
        }

        void IConeSuite.AddTestMethod(ConeMethodThunk thunk) { 
            AddWithAttributes(thunk, new ConeTestMethod(thunk, this, testExecutor, thunk.NameFor(null))); 
        }
        
        void IConeSuite.AddSubsuite(IConeSuite suite) {
            Add((Test)suite);
        }

        void IConeSuite.AddRowTest(string name, MethodInfo method, IEnumerable<IRowData> rows) {
            GetSuite(method, name).Add(rows, testNamer);
        }

        ConeRowSuite GetSuite(MethodInfo method, string name) {
            ConeRowSuite suite;
            var key = method.Name + "." + name;
            if(!rowSuites.TryGetValue(key, out suite)) {
                rowSuites[key] = suite = new ConeRowSuite(new ConeMethodThunk(method, testNamer), this, testExecutor, name);
                AddWithAttributes(method, suite);
            }
            return suite;
        }

        void AddWithAttributes(ICustomAttributeProvider method, Test test) {
            method.Has<PendingAttribute>(x => {
                test.RunState = RunState.Ignored;
                test.IgnoreReason = x[0].Reason;
            });
            method.Has<ExplicitAttribute>(x => {
                test.RunState = RunState.Explicit;
                test.IgnoreReason = x[0].Reason;
            });
            Add(test);
        }
    }
}
 