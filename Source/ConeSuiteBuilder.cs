using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Core;

namespace Cone.Addin
{
    public abstract class ConeSuiteBuilder<TSuite> where TSuite : IConeSuite
    {
        class FixtureDescription : IFixtureDescription
        {
            public IEnumerable<string> Categories { get; set; }
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
            var setup = new ConeFixtureSetup();
            
            setup.Test += (_, e) => suite.AddTestMethod(new ConeMethodThunk(e.Method, testNamer)); 
            setup.RowTest += (_, e) => suite.AddRowTest(testNamer.NameFor(e.Method), e.Method, e.Rows);      

            setup.CollectFixtureMethods(type);
            suite.BindTo(setup.GetFixtureMethods());
            AddNestedContexts(type, suite);
            suite.AddCategories(description.Categories);
            return suite;
        }

        void AddNestedContexts(Type suiteType, IConeSuite suite) {
            var description = new FixtureDescription {
                SuiteType = "Context",
                SuiteName = suite.Name
            };
            suiteType.GetNestedTypes(BindingFlags.Public).ForEach(item => {
                ContextAttribute contextDescription;
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
