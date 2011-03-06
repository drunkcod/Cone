using System;
using NUnit.Core;

namespace Cone.Addin
{
    public class AddinSuiteBuilder : ConeSuiteBuilder<ConeSuite>
    {
       protected override ConeSuite NewSuite(Type type, IFixtureDescription description, ConeTestNamer testNamer) {
            return new ConeSuite(type, description.SuiteName, description.TestName, description.SuiteType, testNamer);
        }
    }
}
