using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Core;

namespace Cone.Addin
{
    public class ConeSuite : TestSuite, IConeTest
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);

        readonly Type type;
        MethodInfo[] afterEachWithResult;

        class FixtureSetup
        {
            class RowTestCase
            {
                public readonly MethodInfo Method;
                public readonly RowAttribute[] Rows;

                public RowTestCase(MethodInfo method, RowAttribute[] rows) {
                    Method = method;
                    Rows = rows;
                }
            }

            public List<MethodInfo> BeforeAll = new List<MethodInfo>();
            public List<MethodInfo> BeforeEach = new List<MethodInfo>();
            public List<MethodInfo> AfterEach = new List<MethodInfo>();
            public List<MethodInfo> AfterAll = new List<MethodInfo>();
            
            public readonly List<MethodInfo> AfterEachWithResult = new List<MethodInfo>();
            readonly List<MethodInfo> Tests = new List<MethodInfo>();
            readonly List<RowTestCase> RowTests = new List<RowTestCase>();

            public void CollectFixtureMethod(MethodInfo method) {
                if (method.DeclaringType == typeof(object))
                    return;
                var parms = method.GetParameters();
                if (parms.Length == 0)
                    CollectNiladic(method);
                else 
                    CollectWithArguments(method, parms);
            }

            private void CollectNiladic(MethodInfo method) {
                var attributes = method.GetCustomAttributes(true);
                var isTest = true;
                for (int i = 0; i != attributes.Length; ++i) {
                    var x = attributes[i];
                    if (x is BeforeEachAttribute) {
                        BeforeEach.Add(method); isTest = false;
                    } else if (x is AfterEachAttribute) {
                        AfterEach.Add(method); isTest = false;
                    } else if (x is BeforeAllAttribute) {
                        BeforeAll.Add(method); isTest = false;
                    } else if (x is AfterAllAttribute) {
                        AfterAll.Add(method); isTest = false;
                    }
                }
                if (isTest)
                    Tests.Add(method);
            }

            public void AddTestsTo(ConeSuite suite) {
                Tests.ForEach(item => suite.Add(new ConeTestMethod(item, suite, NameFor(item))));
                RowTests.ForEach(item => suite.Add(new ConeRowSuite(item.Method, item.Rows, suite, NameFor(item.Method))));
            }

            void CollectWithArguments(MethodInfo method, ParameterInfo[] parms) {
                if (!method.Has<RowAttribute>(rows => RowTests.Add(new RowTestCase(method, rows)))
                && parms.Length == 1
                && typeof(ITestResult).IsAssignableFrom(parms[0].ParameterType)
                && method.Has<AfterEachAttribute>())
                    AfterEachWithResult.Add(method);
            }
        }

        public static TestSuite For(Type type) {
            var description = DescriptionOf(type);
            return For(type, description.ParentSuiteName, description.TestName).AddCategories(description);
        }

        public static ConeSuite For(Type type, string parentSuiteName, string name) {
            var suite = new ConeSuite(type, parentSuiteName, name);
            var setup = new FixtureSetup();
            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                setup.CollectFixtureMethod(item);

            suite.BindTo(setup);
            suite.AddNestedContexts();
            setup.AddTestsTo(suite);
            return suite;
        }

        ConeSuite(Type type, string parentSuiteName, string name) : base(parentSuiteName, name) {
            this.type = type;
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
            if (methods == null)
                return;
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

        static string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        internal static string NameFor(MethodInfo method, object[] parameters) {
            if (parameters == null)
                return NameFor(method);
            var baseName = NameFor(method);
            var displayArguments = new string[parameters.Length];
            for (int i = 0; i != parameters.Length; ++i)
                displayArguments[i] = parameters[i].ToString();
            return baseName + "(" + string.Join(", ", displayArguments) + ")";
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
                    Add(For(item, TestName.FullName, context.Context).AddCategories(context));
            }
        }

        void BindTo(FixtureSetup setup) {
            fixtureSetUpMethods = setup.BeforeAll.ToArray();
            setUpMethods = setup.BeforeEach.ToArray();
            tearDownMethods = setup.AfterEach.ToArray();
            afterEachWithResult = setup.AfterEachWithResult.ToArray();
            fixtureTearDownMethods = setup.AfterAll.ToArray();
        }

        ConeSuite AddCategories(ContextAttribute context) {
            if(!string.IsNullOrEmpty(context.Category))
                foreach(var category in context.Category.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    Categories.Add(category.Trim());
            return this;
        }
    }
}
