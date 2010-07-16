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
            internal List<MethodInfo> BeforeAll = new List<MethodInfo>();
            internal List<MethodInfo> BeforeEach = new List<MethodInfo>();
            internal List<MethodInfo> AfterEach = new List<MethodInfo>();
            internal List<MethodInfo> AfterAll = new List<MethodInfo>();

            public bool CollectFixtureMethod(MethodInfo method) {
                var item = new MethodMarks(method);
                var marks = 0;
                while(item.MoveNext()) {
                    marks += item.AddIfMarked<BeforeEachAttribute>(BeforeEach);
                    marks += item.AddIfMarked<AfterEachAttribute>(AfterEach);
                    marks += item.AddIfMarked<BeforeAllAttribute>(BeforeAll);
                    marks += item.AddIfMarked<AfterAllAttribute>(AfterAll);
                }
                return marks != 0;
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
            return For(type, ParentFor(type), NameFor(type));
        }

        public static TestSuite For(Type type, string parentSuiteName, string name) {
            var suite = new ConeSuite(type, parentSuiteName, name);
            var setup = new FixtureSetup();

            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                if (!setup.CollectFixtureMethod(item) && item.DeclaringType == type) {
                    var parms = new ParameterSet {
                        TestName = NameFor(item)
                    };
                    suite.Add(NUnitTestCaseBuilder.BuildSingleTestMethod(item, suite, parms));
                }
            }
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

        static string ParentFor(Type type) {
            return DescriptionOf(type).DescribedType.Namespace;
        }

        static string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        static string NameFor(Type type) {
            DescribeAttribute desc;
            if (type.TryGetAttribute<DescribeAttribute>(out desc)) {
                if (string.IsNullOrEmpty(desc.Context))
                    return desc.DescribedType.Name;
                return desc.DescribedType.Name + " - " + desc.Context;
            }
            throw new NotSupportedException();
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
                    Add(For(item, TestName.FullName, context.Context));
            }
        }

        void BindTo(FixtureSetup setup) {
            fixtureSetUpMethods = setup.BeforeAll.ToArray();
            setUpMethods = setup.BeforeEach.ToArray();
            tearDownMethods = setup.AfterEach.ToArray();
            fixtureTearDownMethods = setup.AfterAll.ToArray();
        }

    }
}
