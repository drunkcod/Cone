using System.Reflection;
using NUnit.Core;
using NUnit.Framework;
using Cone.Core;

namespace Cone.Addin
{
    static class NUnitTestExtensions
    {
        public static void ProcessExplicitAttributes(this Test self, IConeAttributeProvider attributes) {
            attributes.Has<ExplicitAttribute>(x => {
                self.RunState = RunState.Explicit;
                self.IgnoreReason = x[0].Reason;
            });
        }
    }
}
