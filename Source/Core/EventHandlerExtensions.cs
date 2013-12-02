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

		public static void Raise<T>(this EventHandler<T> self, object sender, T e) where T : EventArgs {
			if(self == null)
				return;
			self(sender, e);
		}
    }
}
