using System;
using System.Threading;

namespace Cone.Helpers
{
    public class EventSpy 
    {
        static int nextSequenceNumber;

        int sequenceNumber;

        public bool HasBeenRaised { get { return sequenceNumber != 0; } }

        public bool RaisedBefore(EventSpy other) { return sequenceNumber < other.sequenceNumber; }

        protected void Raised() {
            sequenceNumber = Interlocked.Increment(ref nextSequenceNumber);
        }
    }

    public class EventSpy<T> : EventSpy where T : EventArgs
    {
        public static implicit operator EventHandler<T>(EventSpy<T> self) {
            return (s, e) => self.Raised();
        }
    }
}
