using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Core;

namespace Cone.Addin
{
    public class ConeSuite : TestSuite, IConeTest
    {
        readonly Type type;
        MethodInfo[] afterEachWithResult;

        class ConeFixtureMethods
        {
            public MethodInfo[] BeforeAll;
            public MethodInfo[] BeforeEach;
            public MethodInfo[] AfterEach;
            public MethodInfo[] AfterAll;
            public MethodInfo[] AfterEachWithResult;
        }

        class ConeFixtureSetup
        {
            [Flags]
            enum MethodMarks
            {
                None,
                Test = 1,
                BeforeAll = 1 << 2,
                BeforeEach = 1 << 3,
                AfterEach = 1 << 4,
                AfterAll = 1 << 5,
                AfterEachWithResult = 1 << 6
            }

            MethodInfo[] methods;
            MethodMarks[] marks;
            ConeSuite suite;
            int beforeAllCount, beforeEachCount, afterEachCount, afterEachWithResultCount, afterAllCount;

            public ConeFixtureSetup(ConeSuite suite) {
                this.suite = suite;
            }

            public void CollectFixtureMethods(Type type) {
                methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                marks = new MethodMarks[methods.Length];             
                ResetCounts();

                for (int i = 0; i != methods.Length; ++i)
                    ClassifyMethod(i);
            }

            public ConeFixtureMethods GetFixtureMethods() {
                var x = new ConeFixtureMethods();
                x.BeforeAll = new MethodInfo[beforeAllCount];
                x.BeforeEach = new MethodInfo[beforeEachCount];
                x.AfterEach = new MethodInfo[afterEachCount];
                x.AfterEachWithResult = new MethodInfo[afterEachWithResultCount];
                x.AfterAll = new MethodInfo[afterAllCount];

                ResetCounts();

                for (int i = 0; i != methods.Length; ++i) {
                    var method = methods[i];

                    if (MarkedAs(MethodMarks.BeforeAll, i))
                        x.BeforeAll[beforeAllCount++] = method;
                    if (MarkedAs(MethodMarks.BeforeEach, i))
                        x.BeforeEach[beforeEachCount++] = method;
                    if (MarkedAs(MethodMarks.AfterEach, i))
                        x.AfterEach[afterEachCount++] = method;
                    if (MarkedAs(MethodMarks.AfterEachWithResult, i))
                        x.AfterEachWithResult[afterEachWithResultCount++] = method;
                    if (MarkedAs(MethodMarks.AfterAll, i))
                        x.AfterAll[afterAllCount++] = method;
                }

                return x;
            }

            void ClassifyMethod(int index) {
                var method = methods[index];
                if (method.DeclaringType == typeof(object))
                    return;
                var parms = method.GetParameters();
                if (parms.Length == 0)
                    marks[index] = ClassifyNiladic(method);
                else
                    marks[index] = ClassifyWithArguments(method, parms);
            }

            MethodMarks ClassifyNiladic(MethodInfo method) {
                var attributes = method.GetCustomAttributes(true);
                var marks = MethodMarks.None;
                for (int i = 0; i != attributes.Length; ++i) {
                    var x = attributes[i];
                    if (x is BeforeEachAttribute) {
                        marks |= MethodMarks.BeforeEach;
                        ++beforeEachCount;
                    } else if (x is AfterEachAttribute) {
                        marks |= MethodMarks.AfterEach;
                        ++afterEachCount;
                    } else if (x is BeforeAllAttribute) {
                        marks |= MethodMarks.BeforeAll;
                        ++beforeAllCount;
                    } else if (x is AfterAllAttribute) {
                        marks |= MethodMarks.AfterAll;
                        ++afterAllCount;
                    }
                }
                if (marks != MethodMarks.None)
                    return marks;
                suite.Add(new ConeTestMethod(method, suite, ConeTestNamer.NameFor(method)));
                return MethodMarks.Test;
            }

            void ResetCounts() {
                beforeAllCount = beforeEachCount = afterEachCount = afterEachWithResultCount = afterAllCount = 0;
            }

            MethodMarks ClassifyWithArguments(MethodInfo method, ParameterInfo[] parms) {
                if (!method.Has<RowAttribute>(rows => suite.Add(new ConeRowSuite(method, rows, suite, ConeTestNamer.NameFor(method))))
                && parms.Length == 1
                && typeof(ITestResult).IsAssignableFrom(parms[0].ParameterType)
                && method.Has<AfterEachAttribute>()) {
                    ++afterEachWithResultCount;
                    return MethodMarks.AfterEachWithResult;
                }
                else return MethodMarks.None;
            }

            bool MarkedAs(MethodMarks mark, int index) {
                return (marks[index] & mark) != 0;
            }
        }

        public static TestSuite For(Type type) {
            var description = DescriptionOf(type);
            return For(type, description.ParentSuiteName, description.TestName).AddCategories(description);
        }

        public static ConeSuite For(Type type, string parentSuiteName, string name) {
            var suite = new ConeSuite(type, parentSuiteName, name);
            var setup = new ConeFixtureSetup(suite);
            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            suite.AddNestedContexts();
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

        void BindTo(ConeFixtureMethods setup) {
            fixtureSetUpMethods = setup.BeforeAll;
            setUpMethods = setup.BeforeEach;
            tearDownMethods = setup.AfterEach;
            afterEachWithResult = setup.AfterEachWithResult;
            fixtureTearDownMethods = setup.AfterAll;
        }

        ConeSuite AddCategories(ContextAttribute context) {
            if(!string.IsNullOrEmpty(context.Category))
                foreach(var category in context.Category.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    Categories.Add(category.Trim());
            return this;
        }
    }
}
