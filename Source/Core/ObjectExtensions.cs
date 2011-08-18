using System;
using System.Diagnostics;
using System.Globalization;

namespace Cone.Core
{
    public static class ObjectExtensions 
    {
        public static string Inspect(this object obj) {
            return new ObjectInspector(CultureInfo.InvariantCulture).Inspect(obj);
        }

        public static T Timed<T>(this T self, Action<T> action, Action<T, TimeSpan> @finally) {
            var time = Stopwatch.StartNew();
            try {
                action(self);
            } finally {
                @finally(self, time.Elapsed);
            }
            return self;
        }
    }
}
