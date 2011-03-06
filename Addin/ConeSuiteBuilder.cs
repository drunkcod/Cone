using System;
using NUnit.Core;

namespace Cone.Addin
{
    public class ConeSuiteBuilder
    {
        public static TestSuite BuildSuite(Type suiteType) {
            var description = DescriptionOf(suiteType);
            return BuildSuite(suiteType, description.Category, description.SuiteName, description.SuiteType, description.TestName);
        }

        static ConeSuite BuildSuite(Type type, string categories, string parentSuiteName, string suiteType, string name) {
            var suite = new ConeSuite(type, parentSuiteName, name, suiteType);
            var setup = new ConeFixtureSetup(suite, suite.testNamer);
            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            AddNestedContexts(type, suite);
            suite.AddCategories(categories);
            return suite;
        }

        static void AddNestedContexts(Type suiteType, ConeSuite suite) {
            foreach (var item in suiteType.GetNestedTypes()) {
                ContextAttribute description;
                if (item.TryGetAttribute<ContextAttribute, ContextAttribute>(out description))
                    suite.Add(BuildSuite(item, description.Category, suite.TestName.FullName, "Context", description.Context));
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
