using NUnit.Core;
using NUnit.Core.Extensibility;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

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
            return true;
        }

        Test ISuiteBuilder.BuildFrom(Type type) { return SuiteBuilder.BuildSuite(type); }

        bool ISuiteBuilder.CanBuildFrom(Type type) { return type.IsPublic && SuiteBuilder.SupportedType(type); }
    }
}
