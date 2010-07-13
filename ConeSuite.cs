using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Core;
using NUnit.Core.Builders;
using NUnit.Core.Extensibility;
using System.Collections.Generic;

namespace Cone
{
    class ConeSuite : TestSuite
    {
        static readonly Regex normalizeNamePattern = new Regex(@"_|\+", RegexOptions.Compiled);
        readonly Type type;

        public static TestSuite For(Type type) {
            var suite = new ConeSuite(type);

            var beforeAll = new List<MethodInfo>();
            var beforeEach = new List<MethodInfo>();
            var afterEach = new List<MethodInfo>();
            var afterAll = new List<MethodInfo>();

            foreach (var item in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                if (item.GetCustomAttributes(typeof(BeforeEachAttribute), true).Length > 0)
                    beforeEach.Add(item);
                else if (item.GetCustomAttributes(typeof(BeforeAllAttribute), true).Length > 0)
                    beforeAll.Add(item);
                else if (item.GetCustomAttributes(typeof(AfterEachAttribute), true).Length > 0)
                    afterEach.Add(item);
                else if (item.GetCustomAttributes(typeof(AfterAllAttribute), true).Length > 0)
                    afterAll.Add(item);
                else if (item.DeclaringType == type) {
                    var parms = new ParameterSet {
                        TestName = NameFor(item)
                    };
                    suite.Add(NUnitTestCaseBuilder.BuildSingleTestMethod(item, suite, parms));
                }
            }
            suite.fixtureSetUpMethods = beforeAll.ToArray();
            suite.setUpMethods = beforeEach.ToArray();
            suite.tearDownMethods = afterEach.ToArray();
            suite.fixtureTearDownMethods = afterAll.ToArray();
            foreach (var context in type.GetNestedTypes())
                if (context.Has<ContextAttribute>())
                    suite.Add(For(context));
            return suite;
        }

        ConeSuite(Type type)
            : base(ParentFor(type), NameFor(type)) {
            this.type = type;
        }

        public override Type FixtureType {
            get { return type; }
        }

        static string NameFor(MethodInfo method) {
            return normalizeNamePattern.Replace(method.Name, " ");
        }

        static string ParentFor(Type type) {
            return DescriptionOf(type).DescribedType.Namespace;
        }

        static string NameFor(Type type) {
            DescribeAttribute desc;
            if (type.TryGetAttribute<DescribeAttribute>(out desc)) {
                if (string.IsNullOrEmpty(desc.Context))
                    return desc.DescribedType.Name;
                return desc.DescribedType.Name + " - " + desc.Context;
            }
            ContextAttribute context;
            type.TryGetAttribute<ContextAttribute>(out context);
            return context.Context;
        }

        static DescribeAttribute DescriptionOf(Type type) {
            DescribeAttribute desc;
            if (!type.TryGetAttribute<DescribeAttribute>(out desc))
                type.DeclaringType.TryGetAttribute<DescribeAttribute>(out desc);
            return desc;
        }
    }
}
