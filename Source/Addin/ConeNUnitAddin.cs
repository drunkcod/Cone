using System;
using Cone.Core;
using NUnit.Core;
using NUnit.Core.Extensibility;
using NUnit.Framework;

namespace Cone.Addin
{
    [NUnitAddin(Name= "Cone")]
    public class ConeNUnitAddin : IAddin, ISuiteBuilder
    {
        readonly AddinSuiteBuilder SuiteBuilder = new AddinSuiteBuilder();

        bool IAddin.Install(IExtensionHost host) {
            var suiteBuilders = host.GetExtensionPoint("SuiteBuilders");
            if (suiteBuilders == null)
                return false;
            suiteBuilders.Install(this);
            Verify.ExpectationFailed = AssertionFailed;
            return true;
        }

        Test ISuiteBuilder.BuildFrom(Type type) { return SuiteBuilder.BuildSuite(type); }

        bool ISuiteBuilder.CanBuildFrom(Type type) { return type.IsPublic && SuiteBuilder.SupportedType(type); }

        static void AssertionFailed(string message, Maybe<object> actual, Maybe<object> expected, Exception innerException) {
            throw new AssertionException(message, innerException);
        }
    }
}
