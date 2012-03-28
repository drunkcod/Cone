using System;
using System.Collections.Generic;

namespace Cone.Core
{
    static class MissingLinqExtensions
    {
        public static void ForEach<T>(IEnumerable<T> self, Action<T> action) {
            foreach(var item in self)
                action(item);
        }
    }
}
