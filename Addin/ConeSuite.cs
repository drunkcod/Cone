﻿using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Core;
using NUnit.Framework;

namespace Cone.Addin
{
    public class ConeSuite : TestSuite, IConeFixture, IConeSuite
    {
        readonly Type type;
        readonly TestExecutor testExecutor;
        readonly ConeTestNamer testNamer;
        readonly Dictionary<string,ConeRowSuite> rowSuites = new Dictionary<string,ConeRowSuite>();
        readonly string suiteType;
        MethodInfo[] afterEachWithResult;

        internal ConeSuite(Type type, string parentSuiteName, string name, string suiteType, ConeTestNamer testNamer) : base(parentSuiteName, name) {
            this.type = type;
            this.testExecutor = new TestExecutor(this);
            this.suiteType = suiteType;
            this.testNamer = testNamer;
        }

        public string Name { get { return TestName.FullName; } }

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
 