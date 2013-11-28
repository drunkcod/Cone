using System;
using Cone.Core;

namespace Cone.Runners
{
    public class ConePadSuiteBuilder : ConeSuiteBuilder<ConePadSuite>
    {
		readonly ObjectProvider fixtureCreator = new DefaultObjectProvider();
        protected override ConePadSuite NewSuite(Type type, IFixtureDescription description) {
            return new ConePadSuite(new ConeFixture(type, description.Categories, fixtureCreator)) { 
				Name = description.SuiteName + "." + description.TestName
			};
        }

        protected override void AddSubSuite(ConePadSuite suite, Lazy<ConePadSuite> subsuite) {
            suite.AddSubSuite(subsuite);
        }
    }
}
