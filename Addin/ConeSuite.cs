using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Core;

namespace Cone.Addin
{
    public class ConeSuite : TestSuite, IConeFixture, IConeSuite
    {
        readonly Type type;
        readonly TestExecutor testExecutor;
        MethodInfo[] afterEachWithResult;

        public static TestSuite For(Type type) {
            var description = DescriptionOf(type);
            return For(type, description, description.ParentSuiteName, description.TestName);
        }

        public static ConeSuite For(Type type, ContextAttribute context, string parentSuiteName, string name) {
            var suite = new ConeSuite(type, parentSuiteName, name);
            var setup = new ConeFixtureSetup(suite);
            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            suite.AddNestedContexts();
            suite.AddCategories(context);
            return suite;
        }

        ConeSuite(Type type, string parentSuiteName, string name) : base(parentSuiteName, name) {
            this.type = type;
            this.testExecutor = new TestExecutor(this);
        }

        public void After(ITestResult testResult) {
            FixtureInvokeAll(afterEachWithResult, new[] { testResult });
            FixtureInvokeAll(tearDownMethods, null);
        }

        public void Before() {
            if(Fixture == null)
                Fixture = FixtureType.GetConstructor(Type.EmptyTypes).Invoke(null);
            FixtureInvokeAll(setUpMethods, null);
        }

        void FixtureInvokeAll(MethodInfo[] methods, object[] parameters) {
            for (int i = 0; i != methods.Length; ++i)
                methods[i].Invoke(Fixture, parameters);
        }

        public override Type FixtureType {
            get { return type; }
        }

        public ConeSuite AddSubSuite(Type fixtureType, string name) {
            var subSuite = new ConeSuite(fixtureType, TestName.FullName, name);
            subSuite.setUpMethods = setUpMethods;
            subSuite.tearDownMethods = tearDownMethods;
            subSuite.afterEachWithResult = afterEachWithResult;
            Add(subSuite);
            return subSuite;
        }

        static DescribeAttribute DescriptionOf(Type type) {
            DescribeAttribute desc;
            if (!type.TryGetAttribute<DescribeAttribute>(out desc))
                throw new NotSupportedException();
            return desc;
        }

        void AddNestedContexts() {
            foreach (var item in type.GetNestedTypes()) {
                ContextAttribute context;
                if (item.TryGetAttribute<ContextAttribute>(out context))
                    Add(For(item, context, TestName.FullName, context.Context));
            }
        }

        void BindTo(ConeFixtureMethods setup) {
            fixtureSetUpMethods = setup.BeforeAll;
            setUpMethods = setup.BeforeEach;
            tearDownMethods = setup.AfterEach;
            afterEachWithResult = setup.AfterEachWithResult;
            fixtureTearDownMethods = setup.AfterAll;
        }

        void AddCategories(ContextAttribute context) {
            if(!string.IsNullOrEmpty(context.Category))
                foreach(var category in context.Category.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    Categories.Add(category.Trim());
        }

        void IConeSuite.AddTestMethod(MethodInfo method) { Add(new ConeTestMethod(method, this, testExecutor, ConeTestNamer.NameFor(method))); }
        void IConeSuite.AddRowTest(MethodInfo method, RowAttribute[] rows) { Add(new ConeRowSuite(method, rows, this, testExecutor, ConeTestNamer.NameFor(method))); }
    }
}
