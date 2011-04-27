using System;
using NUnit.Core;

namespace Cone.Addin
{
    public class AddinSuiteBuilder : ConeSuiteBuilder<AddinSuite>
    {
        protected override AddinSuite NewSuite(Type type, IFixtureDescription description, ConeTestNamer testNamer) {
            var suite = new AddinSuite(type, description.SuiteName, description.TestName, description.SuiteType, testNamer);
            suite.Has<IPendingAttribute>(x => suite.Ignore(x[0].Reason));
            return suite;
        }
    }
}
