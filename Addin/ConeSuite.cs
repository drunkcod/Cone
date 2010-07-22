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
            public List<MethodInfo> BeforeAll = new List<MethodInfo>();
            public List<MethodInfo> BeforeEach = new List<MethodInfo>();
            public List<MethodInfo> AfterEach = new List<MethodInfo>();
            public List<MethodInfo> AfterAll = new List<MethodInfo>();
            
            readonly List<MethodInfo> AfterEachWithResult = new List<MethodInfo>();
            readonly List<MethodInfo> Tests = new List<MethodInfo>();

            public bool CollectFixtureMethod(MethodInfo method) {
                var parms = method.GetParameters();
                if (parms.Length != 0)
                    return CollectAfterEachWithResult(method, parms);
                var item = new MethodMarks(method);
                var marks = 0;
                while(item.MoveNext()) {
                    marks += item.AddIfMarked<BeforeEachAttribute>(BeforeEach);
                    marks += item.AddIfMarked<AfterEachAttribute>(AfterEach);
                    marks += item.AddIfMarked<BeforeAllAttribute>(BeforeAll);
                    marks += item.AddIfMarked<AfterAllAttribute>(AfterAll);
                }
                if (marks == 0 && method.DeclaringType != typeof(object))
                    Tests.Add(method);
                return marks != 0;
            }

            public void AddTestsTo(TestSuite suite) {
                if (AfterEachWithResult.Count == 0)
                    foreach (var item in Tests)
                        suite.Add(new ConeTestMethod(item, suite, NameFor(item)));
                else {
                    var afterEachWithResultArray = AfterEachWithResult.ToArray();
                    foreach (var item in Tests)
                        suite.Add(new ReportingConeTestMethod(item, suite, NameFor(item), afterEachWithResultArray));
                }
            }

            bool CollectAfterEachWithResult(MethodInfo method, ParameterInfo[] parms) {
                if (parms.Length == 1
                && typeof(ITestResult).IsAssignableFrom(parms[0].ParameterType)
                && method.Has<AfterEachAttribute>()) {
                    AfterEachWithResult.Add(method);
                    return true;
                }                
                return false;
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

        class ConeTestMethod : NUnitTestMethod
        {
            public ConeTestMethod(MethodInfo method, Test suite, string name) : base(method) {
                this.Parent = suite;
                this.TestName.FullName = suite.TestName.FullName + "." + name;
                this.TestName.Name = name;
            }

            public override void doRun(TestResult testResult) {
                base.doRun(testResult);
                After(testResult);
            }

            protected virtual void After(TestResult testResult) { }
        }

        class ReportingConeTestMethod : ConeTestMethod
        {
            readonly MethodInfo[] afters;

            public ReportingConeTestMethod(MethodInfo method, TestSuite suite, string name, MethodInfo[] afters) : base(method, suite, name) {
                this.afters = afters;
            }

            protected override void After(TestResult testResult) {
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

        public override Type FixtureType {
            get { return type; }
        }

        static string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
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
