using System;

namespace Cone.Core
{
    static class EventHandlerExtensions
    {
        public static void Raise(this EventHandler self, object sender, EventArgs e) {
            if(self == null)
                return;
            self(sender, e);
        }
    }
}
