using System;
using Cone.Core;

namespace Cone.Addin
{
    public class AddinSuiteBuilder : ConeSuiteBuilder<AddinSuite>
    {
        protected override AddinSuite NewSuite(Type type, IFixtureDescription description, ConeTestNamer testNamer) {
            return new AddinSuite(type, description, testNamer);
        }
    }
}
