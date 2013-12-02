using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
	public interface IConeSuiteBuilder<TSuite> where TSuite : IConeSuite
	{
		bool SupportedType(Type type);
		TSuite BuildSuite(Type suiteType);
	}

	public abstract class ConeSuiteBuilder<TSuite> : IConeSuiteBuilder<TSuite> where TSuite : class, IConeSuite
    {
		static readonly Type[] FixtureAttributes = { typeof(DescribeAttribute), typeof(FeatureAttribute) };

		readonly ConeTestNamer names = new ConeTestNamer(); 

        class ContextDescription : IFixtureDescription
        {
            public IEnumerable<string> Categories { get; set; }
            public string SuiteName { get; set; }
            public string SuiteType { get { return "Context"; } }
            public string TestName { get; set; }
        }

		class NullFixtureDescription : IFixtureDescription 
        {
            public IEnumerable<string> Categories {
                get { return new string[0]; }
            }

            public string SuiteName {
                get { return string.Empty; }
            }

            public string SuiteType {
                get { return "TestSuite"; }
            }

            public string TestName {
                get { return string.Empty; }
            }
        }

        public virtual bool SupportedType(Type type) { return type.HasAny(FixtureAttributes); }

        public TSuite BuildSuite(Type suiteType) {
            return BuildSuite(Enumerable.Empty<string>(), suiteType, DescriptionOf(suiteType));
        }

        protected abstract TSuite NewSuite(Type type, IFixtureDescription description);
        protected abstract void AddSubSuite(TSuite suite, TSuite subsuite);

        TSuite BuildSuite(IEnumerable<string> parentCategories, Type type, IFixtureDescription description) {
            var suite = NewSuite(type, description);

			suite.AddCategories(parentCategories);
            suite.AddCategories(description.Categories);
			suite.DiscoverTests(names);
            AddNestedContexts(type, suite);
            return suite;
        }

        void AddNestedContexts(Type suiteType, TSuite suite) {
            ContextAttribute contextDescription;
            suiteType.GetNestedTypes(BindingFlags.Public).ForEach(item => {
                if (item.TryGetAttribute(out contextDescription)) {
					var description = new ContextDescription {
						SuiteName = suite.Name,
	                    Categories = contextDescription.Categories,
						TestName = contextDescription.Context
					};
                    AddSubSuite(suite, BuildSuite(suite.Categories, item, description));
                }
            });
        }

        public virtual IFixtureDescription DescriptionOf(Type fixtureType) {
            return fixtureType.WithAttributes(
                (IFixtureDescription[] x) => x[0], 
                () => new NullFixtureDescription());
        }
    }
}
