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
        readonly ConeFixture fixture;

        internal AddinSuite(Type type, string parentSuiteName, string name, string suiteType, ConeTestNamer testNamer) : base(parentSuiteName, name) {
            this.type = type;
            this.suiteType = suiteType;
            this.testNamer = testNamer;
            this.fixture = new ConeFixture(this);
            this.testExecutor = new TestExecutor(this.fixture);
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
            foreach(var item in rowSources) {
                foreach(IRowTestData row in (IEnumerable<IRowTestData>)fixture.Invoke(item)) {
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

        public void AddCategories(IEnumerable<string> categories) {
            foreach(var item in categories)
                Categories.Add(item);
        }

        void IConeSuite.AddTestMethod(ConeMethodThunk thunk) { 
            AddWithAttributes(thunk, new ConeTestMethod(thunk, this, testExecutor, thunk.NameFor(null))); 
        }
        
        void IConeSuite.AddSubsuite(IConeSuite suite) {
            AddWithAttributes(suite, (Test)suite);
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
            method.Has<IPendingAttribute>(x => test.Ignore(x[0].Reason));
            method.Has<ExplicitAttribute>(x => {
                test.RunState = RunState.Explicit;
                test.IgnoreReason = x[0].Reason;
            });
            Add(test);
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) {
            return type.GetCustomAttributes(inherit);
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(Type attributeType, bool inherit) {
            return type.GetCustomAttributes(attributeType, inherit);
        }

        bool ICustomAttributeProvider.IsDefined(Type attributeType, bool inherit) {
            return type.IsDefined(attributeType, inherit);
        }
    }
}
 