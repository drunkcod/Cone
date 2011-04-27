using System.Reflection;
using NUnit.Core;

namespace Cone.Addin
{
    static class NUnitTestExtensions
    {
        public static void Ignore(this Test self, string reason) {
            self.RunState = RunState.Ignored;
            self.IgnoreReason = reason;
        }

        public static void ProcessPendingAttributes(this Test self, ICustomAttributeProvider attributes) {
            var pending = attributes.FirstOrDefault<IPendingAttribute>(x => x.IsPending);
            if(pending != null)
                self.Ignore(pending.Reason);
        }
    }
}
