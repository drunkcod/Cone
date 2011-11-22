using System.Threading;

namespace Cone.Helpers
{
    public class MethodSpy 
    {
        static int nextSequenceNumber;

        int sequenceNumber;

        public bool HasBeenCalled { get { return sequenceNumber != 0; } }

        public bool CalledBefore(MethodSpy other) { return sequenceNumber < other.sequenceNumber; }

        protected void Called() {
            sequenceNumber = Interlocked.Increment(ref nextSequenceNumber);
        }
    }
}
