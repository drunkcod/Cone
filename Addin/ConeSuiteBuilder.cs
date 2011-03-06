using System;
using NUnit.Core;

namespace Cone.Addin
{
    public class ConeSuiteBuilder
    {
        public static TestSuite BuildSuite(Type suiteType) {
            var description = DescriptionOf(suiteType);
            return BuildSuite(suiteType, description);
        }

        static ConeSuite BuildSuite(Type type, IFixtureDescription description) {
            var suite = new ConeSuite(type, description.SuiteName, description.TestName, description.SuiteType);
            var setup = new ConeFixtureSetup(suite, suite.testNamer);
            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            AddNestedContexts(type, suite);
            suite.AddCategories(description.Category);
            return suite;
        }

        class FixtureDescription : IFixtureDescription
        {
            public string Category { get; set; }
            public string SuiteName { get; set; }
            public string SuiteType { get; set; }
            public string TestName { get; set; }
        }

        static void AddNestedContexts(Type suiteType, ConeSuite suite) {
            var fixtureDescription = new FixtureDescription {
                SuiteType = "Context",
                SuiteName = suite.TestName.FullName
            };
            foreach (var item in suiteType.GetNestedTypes()) {
                ContextAttribute description;
                if (item.TryGetAttribute<ContextAttribute, ContextAttribute>(out description)) {
                    fixtureDescription.Category = description.Category;
                    fixtureDescription.TestName = description.Context;
                    suite.Add(BuildSuite(item, fixtureDescription));
                }
            }
        }

        public static bool SupportedType(Type type) { return type.IsPublic && (type.Has<DescribeAttribute>() || type.Has<FeatureAttribute>()); } 
    
        static IFixtureDescription DescriptionOf(Type fixtureType) {
            IFixtureDescription desc;
            if (fixtureType.TryGetAttribute<DescribeAttribute, IFixtureDescription>(out desc)
                || fixtureType.TryGetAttribute<FeatureAttribute, IFixtureDescription>(out desc))
                return desc;
            throw new NotSupportedException();
        }

    }
}
