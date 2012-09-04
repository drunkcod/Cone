using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
	static class IConeSuiteExtensions
	{
		public static void DiscoverTests(this IConeSuite self, ConeTestNamer names, Type type) {
            self.WithTestMethodSink(names, testSink =>
            self.WithFixtureMethodSink(fixtureSink => {
                var setup = new ConeFixtureSetup(fixtureSink, testSink);
                setup.CollectFixtureMethods(type);
            }));
		}
	}

	public class Lazy<T>
	{
		Func<T> getValue;
 
		public Lazy(Func<T> forceValue) {
			getValue = () => {
				var value = forceValue();
				getValue = () => value;
				return value;
			};
		}

		public T Value { get { return getValue(); } }
	}

    public abstract class ConeSuiteBuilder<TSuite> where TSuite : IConeSuite
    {
		static readonly Type[] FixtureAttributes = new[]{ typeof(DescribeAttribute), typeof(FeatureAttribute) };
        readonly ConeTestNamer names = new ConeTestNamer(); 

        class ContextDescription : IFixtureDescription
        {
            public IEnumerable<string> Categories { get; set; }
            public string SuiteName { get; set; }
            public string SuiteType { get { return "Context"; } }
            public string TestName { get; set; }
        }

        public static bool SupportedType(Type type) { return type.HasAny(FixtureAttributes); } 

        public TSuite BuildSuite(Type suiteType) {
            return BuildSuite(suiteType, DescriptionOf(suiteType));
        }

        protected abstract TSuite NewSuite(Type type, IFixtureDescription description);
        protected abstract void AddSubSuite(TSuite suite, Lazy<TSuite> subsuite);

        TSuite BuildSuite(Type type, IFixtureDescription description) {
            var suite = NewSuite(type, description);
            suite.AddCategories(description.Categories);
			suite.DiscoverTests(names, type);
            AddNestedContexts(type, suite);
            return suite;
        }

        void AddNestedContexts(Type suiteType, TSuite suite) {
            ContextAttribute contextDescription;
            suiteType.GetNestedTypes(BindingFlags.Public).ForEach(item => {
                if (item.TryGetAttribute<ContextAttribute, ContextAttribute>(out contextDescription)) {
					var description = new ContextDescription {
						SuiteName = suite.Name,
	                    Categories = contextDescription.Categories,
						TestName = contextDescription.Context
					};
                    AddSubSuite(suite, new Lazy<TSuite>(() => BuildSuite(item, description)));
                }
            });
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

        protected static IFixtureDescription DescriptionOf(Type fixtureType) {
            return fixtureType.WithAttributes(
                (IFixtureDescription[] x) => x[0], 
                () => new NullFixtureDescription());
        }
    }
}
