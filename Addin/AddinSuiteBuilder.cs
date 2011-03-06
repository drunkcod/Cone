using System;
using NUnit.Core;

namespace Cone.Addin
{
    public class AddinSuiteBuilder : ConeSuiteBuilder
    {
        public TestSuite BuildSuite(Type suiteType) {
            var description = DescriptionOf(suiteType);
            return (TestSuite)BuildSuite(suiteType, description);
        }

        protected override IConeSuite NewSuite(Type type, IFixtureDescription description, ConeTestNamer testNamer) {
            return new ConeSuite(type, description.SuiteName, description.TestName, description.SuiteType, testNamer);
        }
    }
}
