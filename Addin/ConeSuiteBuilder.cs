using System;
using System.Reflection;

namespace Cone.Addin
{
    public abstract class ConeSuiteBuilder<TSuite> where TSuite : IConeSuite
    {
        class FixtureDescription : IFixtureDescription
        {
            public string Category { get; set; }
            public string SuiteName { get; set; }
            public string SuiteType { get; set; }
            public string TestName { get; set; }
        }

        public bool SupportedType(Type type) { return type.IsPublic && (type.Has<DescribeAttribute>() || type.Has<FeatureAttribute>()); } 

        public TSuite BuildSuite(Type suiteType) {
            var description = DescriptionOf(suiteType);
            return BuildSuite(suiteType, description);
        }

        protected abstract TSuite NewSuite(Type type, IFixtureDescription description, ConeTestNamer testNamer);

        protected TSuite BuildSuite(Type type, IFixtureDescription description) {
            var testNamer = new ConeTestNamer();
            var suite = NewSuite(type, description, testNamer);
            var setup = new ConeFixtureSetup(suite, testNamer);
            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            AddNestedContexts(type, suite);
            suite.AddCategories(description.Category);
            return suite;
        }

        void AddNestedContexts(Type suiteType, IConeSuite suite) {
            var description = new FixtureDescription {
                SuiteType = "Context",
                SuiteName = suite.Name
            };
            foreach (var item in suiteType.GetNestedTypes(BindingFlags.Public)) {
                ContextAttribute contextDescription;
                if (item.TryGetAttribute<ContextAttribute, ContextAttribute>(out contextDescription)) {
                    description.Category = contextDescription.Category;
                    description.TestName = contextDescription.Context;
                    suite.AddSubsuite(BuildSuite(item, description));
                }
            }
        }

        protected static IFixtureDescription DescriptionOf(Type fixtureType) {
            IFixtureDescription desc;
            if (fixtureType.TryGetAttribute<DescribeAttribute, IFixtureDescription>(out desc)
                || fixtureType.TryGetAttribute<FeatureAttribute, IFixtureDescription>(out desc))
                return desc;
            throw new NotSupportedException();
        }
    }
}
