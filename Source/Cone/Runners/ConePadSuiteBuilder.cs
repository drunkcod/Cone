using System;
using System.Collections.Generic;
using Cone.Core;

namespace Cone.Runners
{
	public class ConePadSuiteBuilder : ConeSuiteBuilder<ConeSuite>
	{
		readonly FixtureProvider objectProvider;

		public ConePadSuiteBuilder(ITestNamer testNamer, FixtureProvider objectProvider) : base(testNamer) {
			this.objectProvider = objectProvider;
		}
		
		protected override ConeSuite NewSuite(Type type, IFixtureDescription description) =>
			new ConeSuite(MakeFixture(type, description.Categories), description.SuiteName + "." + description.TestName);

		protected ConeFixture MakeFixture(Type type, IEnumerable<string> categories)=>
			new ConeFixture(type, categories, objectProvider);

		protected override void AddSubSuite(ConeSuite suite, ConeSuite subsuite) =>
			suite.AddSubSuite(subsuite);
	}
}
