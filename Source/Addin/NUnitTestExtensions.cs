using System.Reflection;
using NUnit.Core;
using NUnit.Framework;

namespace Cone.Addin
{
    static class NUnitTestExtensions
    {
        public static void Ignore(this Test self, RunState runState, string reason) {
            self.RunState = runState;
            self.IgnoreReason = reason;
        }

        public static void ProcessPendingAttributes(this Test self, ICustomAttributeProvider attributes) {
            var pending = attributes.FirstOrDefault<IPendingAttribute>(x => x.IsPending);
            if(pending != null)
                self.Ignore(RunState.Ignored, pending.Reason);
        }

        public static void ProcessExplicitAttributes(this Test self, ICustomAttributeProvider attributes) {
            attributes.Has<ExplicitAttribute>(x => {
                self.Ignore(RunState.Explicit, x[0].Reason);
            });
        }
    }
}
