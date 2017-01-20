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

		readonly ITestNamer testNamer; 

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
				get { return Enumerable.Empty<string>(); }
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

		public ConeSuiteBuilder(ITestNamer testNamer) { 
			this.testNamer = testNamer;	
		}

		public virtual bool SupportedType(Type type) { return type.HasAny(FixtureAttributes); }

		public TSuite BuildSuite(Type suiteType) {
			return BuildSuite(suiteType, DescriptionOf(suiteType));
		}

		protected abstract TSuite NewSuite(Type type, IFixtureDescription description);
		protected abstract void AddSubSuite(TSuite suite, TSuite subsuite);

		TSuite BuildSuite(Type type, IFixtureDescription description) {
			var suite = NewSuite(type, description);

			suite.AddCategories(description.Categories);
			suite.DiscoverTests(testNamer);
			AddNestedContexts(type, suite);
			return suite;
		}

		void AddNestedContexts(Type suiteType, TSuite suite) {
			IContextDescription contextDescription;
			suiteType.GetNestedTypes(BindingFlags.Public).ForEach(item => {
				if (TryGetContext(item, out contextDescription)) {
					var description = new ContextDescription {
						SuiteName = suite.Name,
						Categories = suite.Categories.Concat(contextDescription.Categories),
						TestName = contextDescription.Context
					};
						AddSubSuite(suite, BuildSuite(item, description));
				}
			});
		}

		protected virtual bool TryGetContext(Type nestedType, out IContextDescription context) {
			ContextAttribute attr;
			var result = nestedType.TryGetAttribute(out attr);
			context = attr;
			return result;
		}

		public virtual IFixtureDescription DescriptionOf(Type fixtureType) {
			return fixtureType.WithAttributes(
				(IFixtureDescription[] x) => x[0], 
				() => new NullFixtureDescription());
		}
	}
}
