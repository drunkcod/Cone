using System;
using Cone.Core;

namespace Cone.Runners
{
    class ConePadSuiteBuilder : ConeSuiteBuilder<ConePadSuite>
    {
        protected override ConePadSuite NewSuite(Type type, IFixtureDescription description) {
            return new ConePadSuite(new ConeFixture(type)) { Name = description.TestName };
        }

        protected override void AddSubSuite(ConePadSuite suite, ConePadSuite subsuite) {
            suite.AddSubSuite(subsuite);
        }
    }
}
