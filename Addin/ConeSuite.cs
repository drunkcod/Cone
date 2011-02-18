using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Core;
using NUnit.Framework;

namespace Cone.Addin
{
    public class ConeSuite : TestSuite, IConeFixture, IConeSuite
    {
        readonly Type type;
        readonly TestExecutor testExecutor;
        readonly ConeTestNamer testNamer = new ConeTestNamer();
        readonly Dictionary<string,ConeRowSuite> rowSuites = new Dictionary<string,ConeRowSuite>();
        string suiteType;
        MethodInfo[] afterEachWithResult;

        public static TestSuite For(Type type) {
            var description = DescriptionOf(type);
            return For(type, description.Category, description.SuiteName, description.SuiteType, description.TestName);
        }

        public static ConeSuite For(Type type, string categories, string parentSuiteName, string suiteType, string name) {
            var suite = new ConeSuite(type, parentSuiteName, name);
            var setup = new ConeFixtureSetup(suite, suite.testNamer);
            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            suite.AddNestedContexts();
            suite.AddCategories(categories);
            suite.suiteType = suiteType;
            return suite;
        }

        public static bool SupportedType(Type type) { return type.IsPublic && (type.Has<DescribeAttribute>() || type.Has<FeatureAttribute>()); }

        ConeSuite(Type type, string parentSuiteName, string name) : base(parentSuiteName, name) {
            this.type = type;
            this.testExecutor = new TestExecutor(this);
        }

        public void After(ITestResult testResult) {
            FixtureInvokeAll(afterEachWithResult, new[] { testResult });
            FixtureInvokeAll(tearDownMethods, null);
        }

        public void Before() {
            if (Fixture == null)
                Fixture = NewFixture();
            FixtureInvokeAll(setUpMethods, null);
        }

        object NewFixture() { 
            var ctor = FixtureType.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
                return null;
            return ctor.Invoke(null);
        }

        void FixtureInvokeAll(MethodInfo[] methods, object[] parameters) {
            for (int i = 0; i != methods.Length; ++i)
                methods[i].Invoke(Fixture, parameters);
        }

        public override Type FixtureType {
            get { return type; }
        }

        public override string TestType { get { return suiteType; } }

        public ConeSuite AddSubSuite(Type fixtureType, string name) {
            var subSuite = new ConeSuite(fixtureType, TestName.FullName, name);
            subSuite.setUpMethods = setUpMethods;
            subSuite.tearDownMethods = tearDownMethods;
            subSuite.afterEachWithResult = afterEachWithResult;
            Add(subSuite);
            return subSuite;
        }

        static IFixtureDescription DescriptionOf(Type fixtureType) {
            IFixtureDescription desc;
            if (fixtureType.TryGetAttribute<DescribeAttribute, IFixtureDescription>(out desc)
                || fixtureType.TryGetAttribute<FeatureAttribute, IFixtureDescription>(out desc))
                return desc;
            throw new NotSupportedException();
        }

        void AddNestedContexts() {
            foreach (var item in type.GetNestedTypes()) {
                ContextAttribute description;
                if (item.TryGetAttribute<ContextAttribute, ContextAttribute>(out description))
                    Add(For(item, description.Category, TestName.FullName, "Context", description.Context));
            }
        }

        void BindTo(ConeFixtureMethods setup) {
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
            var fixture = NewFixture();
            var rows = new Dictionary<MethodInfo, List<IRowData>>();
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

        void AddCategories(string categories) {
            if(!string.IsNullOrEmpty(categories))
                foreach(var category in categories.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    Categories.Add(category.Trim());
        }

        void IConeSuite.AddTestMethod(string name, MethodInfo method) { 
            AddMethod(method, new ConeTestMethod(method, this, testExecutor, name)); 
        }
        
        void IConeSuite.AddRowTest(string name, MethodInfo method, IEnumerable<IRowData> rows) {
            GetSuite(method, name).Add(rows, testNamer);
        }

        ConeRowSuite GetSuite(MethodInfo method, string name) {
            ConeRowSuite suite;
            var key = method.Name + "." + name;
            if(!rowSuites.TryGetValue(key, out suite)) {
                rowSuites[key] = suite = new ConeRowSuite(method, this, testExecutor, name);
                AddMethod(method, suite); 
            }
            return suite;
        }

        void AddMethod(MethodInfo method, Test test) {
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
 