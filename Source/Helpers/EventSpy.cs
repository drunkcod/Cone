using System;

namespace Cone.Helpers
{
    public class EventSpy<T> : MethodSpy where T : EventArgs
    {
        public static implicit operator EventHandler<T>(EventSpy<T> self) {
            return (s, e) => self.Called();
        }
    }
}
