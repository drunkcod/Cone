using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public abstract class ConeSuiteBuilder<TSuite> where TSuite : IConeSuite
    {
        readonly ConeTestNamer names = new ConeTestNamer(); 

        class ContextDescription : IFixtureDescription
        {
            public IEnumerable<string> Categories { get; set; }
            public string SuiteName { get; set; }
            public string SuiteType { get { return "Context"; } }
            public string TestName { get; set; }
        }

        public bool SupportedType(Type type) { return type.IsPublic && type.HasAny(typeof(DescribeAttribute), typeof(FeatureAttribute)); } 

        public TSuite BuildSuite(Type suiteType) {
            return BuildSuite(suiteType, DescriptionOf(suiteType));
        }

        protected abstract TSuite NewSuite(Type type, IFixtureDescription description, ConeTestNamer testNamer);

        protected TSuite BuildSuite(Type type, IFixtureDescription description) {
            var suite = NewSuite(type, description, names);
            var setup = new ConeFixtureSetup(suite);

            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            AddNestedContexts(type, suite);
            suite.AddCategories(description.Categories);
            return suite;
        }

        void AddNestedContexts(Type suiteType, IConeSuite suite) {
            var description = new ContextDescription {
                SuiteName = suite.Name
            };
            ContextAttribute contextDescription;
            suiteType.GetNestedTypes(BindingFlags.Public).ForEach(item => {
                if (item.TryGetAttribute<ContextAttribute, ContextAttribute>(out contextDescription)) {
                    description.Categories = contextDescription.Categories;
                    description.TestName = contextDescription.Context;
                    suite.AddSubsuite(BuildSuite(item, description));
                }
            });
        }

        protected static IFixtureDescription DescriptionOf(Type fixtureType) {
            return fixtureType.WithAttributes(
                (IFixtureDescription[] x) => x[0], 
                () => { throw new NotSupportedException(); });
        }
    }
}
