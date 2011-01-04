using System;
using NUnit.Core;
using NUnit.Core.Extensibility;
using NUnit.Framework;

namespace Cone.Addin
{
    [NUnitAddin(Name= "Cone")]
    public class ConeNUnitAddin : IAddin, ISuiteBuilder
    {
        bool IAddin.Install(IExtensionHost host) {
            var suiteBuilders = host.GetExtensionPoint("SuiteBuilders");
            if (suiteBuilders == null)
                return false;
            suiteBuilders.Install(this);
            Verify.ExpectationFailed = AssertionFailed;
            return true;
        }

        Test ISuiteBuilder.BuildFrom(Type type) { return ConeSuite.For(type); }

        bool ISuiteBuilder.CanBuildFrom(Type type) { return ConeSuite.SupportedType(type); }

        static void AssertionFailed(string message) {
            throw new AssertionException(message);
        }
    }
}
