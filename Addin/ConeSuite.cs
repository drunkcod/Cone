using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Core;
using NUnit.Core.Builders;
using NUnit.Core.Extensibility;

namespace Cone.Addin
{
    public class ConeSuite : TestSuite
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        readonly Type type;

        class FixtureSetup
        {
            static readonly object[] NoArguments = null;
            public List<MethodInfo> BeforeAll = new List<MethodInfo>();
            public List<MethodInfo> BeforeEach = new List<MethodInfo>();
            public List<MethodInfo> AfterEach = new List<MethodInfo>();
            public List<MethodInfo> AfterAll = new List<MethodInfo>();
            
            readonly List<MethodInfo> AfterEachWithResult = new List<MethodInfo>();
            readonly List<MethodInfo> Tests = new List<MethodInfo>();
            readonly List<MethodInfo> RowTests = new List<MethodInfo>();

            public void CollectFixtureMethod(MethodInfo method) {
                var parms = method.GetParameters();
                if (parms.Length != 0)
                    CollectWithArguments(method, parms);
                else 
                    CollectNiladic(method);
            }

            private void CollectNiladic(MethodInfo method) {
                var item = new MethodMarks(method);
                var marks = 0;
                while (item.MoveNext()) {
                    marks += item.AddIfMarked<BeforeEachAttribute>(BeforeEach);
                    marks += item.AddIfMarked<AfterEachAttribute>(AfterEach);
                    marks += item.AddIfMarked<BeforeAllAttribute>(BeforeAll);
                    marks += item.AddIfMarked<AfterAllAttribute>(AfterAll);
                }
                if (marks == 0 && method.DeclaringType != typeof(object))
                    Tests.Add(method);
            }

            public void AddTestsTo(ConeSuite suite) {
                var createTest = GetTestFactory();

                foreach (var item in Tests)
                    suite.Add(createTest(item, NoArguments, suite));

                foreach (var item in RowTests) {
                    var subSuite = new ConeSuite(item.DeclaringType, suite.TestName.FullName, NameFor(item));
                    foreach (var row in item.GetCustomAttributes(typeof(RowAttribute), true)) {
                        var x = (RowAttribute)row;
                        var parameters = x.Parameters;
                        var test = createTest(item, parameters, subSuite);
                        if (x.Pending)
                            test.RunState = RunState.Ignored;
                        subSuite.Add(test);
                    }
                    suite.Add(subSuite);
                }
            }

            Func<MethodInfo, object[], ConeSuite, Test> GetTestFactory()
            {
                if (AfterEachWithResult.Count == 0)
                    return (m, a, s) => new ConeTestMethod(m, a, s, NameFor(m, a));
                else {
                    var afterEachWithResultArray = AfterEachWithResult.ToArray();
                    return (m, a, s) => new ReportingConeTestMethod(m, a, s, NameFor(m, a), afterEachWithResultArray);
                }
            }

            void CollectWithArguments(MethodInfo method, ParameterInfo[] parms) {
                if (method.Has<RowAttribute>())
                    RowTests.Add(method);
                else if (parms.Length == 1
                && typeof(ITestResult).IsAssignableFrom(parms[0].ParameterType)
                && method.Has<AfterEachAttribute>())
                    AfterEachWithResult.Add(method);
            }

            struct MethodMarks
            {
                readonly MethodInfo method;
                readonly object[] attributes;
                object current;
                int n;

                public MethodMarks(MethodInfo method) {
                    this.method = method;
                    this.attributes = method.GetCustomAttributes(true);
                    this.n = 0;
                    this.current = null;
                }

                public bool MoveNext() {
                    var hasMore = n != attributes.Length;
                    if (hasMore)
                        current = attributes[n++];
                    return hasMore;
                }

                public int AddIfMarked<T>(List<MethodInfo> target) {
                    if (!(current is T))
                        return 0;
                    target.Add(method);
                    return 1;
                }
            }
        }

        public static TestSuite For(Type type) {
            var description = DescriptionOf(type);
            return For(type, description.ParentSuiteName, description.TestName).AddCategories(description);
        }

        class NUnitTestResultAdapter : ITestResult
        {
            readonly TestResult result;

            public NUnitTestResultAdapter(TestResult result) {
                this.result = result;
            }

            string ITestResult.TestName {
                get { return result.Test.TestName.Name; }
            }

            TestStatus ITestResult.Status {
                get { return result.ResultState == ResultState.Success ? TestStatus.Success : TestStatus.Failure; }
            }
        }

        class ReportingConeTestMethod : ConeTestMethod
        {
            readonly MethodInfo[] afters;

            public ReportingConeTestMethod(MethodInfo method, object[] parameters, ConeSuite suite, string name, MethodInfo[] afters) : base(method, parameters, suite, name) {
                this.afters = afters;
            }

            protected override void AfterCore(TestResult testResult) {
                var parms = new[] { new NUnitTestResultAdapter(testResult) }; 
                for(int i = 0; i != afters.Length; ++i)
                    try {
                        afters[i].Invoke(Fixture, parms);
                    } catch (TargetInvocationException e) {
                        throw e.InnerException;
                    }
            }
        }

        public static ConeSuite For(Type type, string parentSuiteName, string name) {
            var suite = new ConeSuite(type, parentSuiteName, name);
            var setup = new FixtureSetup();
            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                setup.CollectFixtureMethod(item); 

            setup.AddTestsTo(suite);
            suite.BindTo(setup);
            suite.AddNestedContexts();
            return suite;
        }

        ConeSuite(Type type, string parentSuiteName, string name) : base(parentSuiteName, name) {
            this.type = type;
        }

        public void After() {
            FixtureInvokeAll(tearDownMethods);
        }

        public void Before() {
            if(Fixture == null)
                Fixture = FixtureType.GetConstructor(Type.EmptyTypes).Invoke(null);
            FixtureInvokeAll(setUpMethods);
        }

        void FixtureInvokeAll(MethodInfo[] methods) {
            if (methods == null)
                return;
            for (int i = 0; i != methods.Length; ++i)
                methods[i].Invoke(Fixture, null);
        }

        public override Type FixtureType {
            get { return type; }
        }

        static string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        static string NameFor(MethodInfo method, object[] arguments) {
            if (arguments == null)
                return NameFor(method);
            var baseName = NameFor(method);
            var displayArguments = new string[arguments.Length];
            for (int i = 0; i != arguments.Length; ++i)
                displayArguments[i] = arguments[i].ToString();
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
