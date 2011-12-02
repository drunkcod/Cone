using System;
using Cone.Core;

namespace Cone.Addin
{
    public class AddinSuiteBuilder : ConeSuiteBuilder<AddinSuite>
    {
        protected override AddinSuite NewSuite(Type type, IFixtureDescription description) {
            return new AddinSuite(type, description);
        }

        protected override void AddSubSuite(AddinSuite suite, AddinSuite subsuite) {
            suite.AddSubSuite(subsuite);
        }
    }
}
