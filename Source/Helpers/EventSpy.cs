using System;

namespace Cone.Helpers
{
    public class EventSpy<T> where T : EventArgs
    {
        bool hasBeenRaised;

        public bool HasBeenRaised { get { return hasBeenRaised; } }

        public static implicit operator EventHandler<T>(EventSpy<T> self) {
            return (s, e) => self.hasBeenRaised = true;
        }
    }
}
