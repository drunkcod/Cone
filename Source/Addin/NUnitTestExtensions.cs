using NUnit.Core;

namespace Cone.Addin
{
    static class NUnitTestExtensions
    {
        public static void Ignore(this Test self, string reason) {
            self.RunState = RunState.Ignored;
            self.IgnoreReason = reason;
        }
    }
}
