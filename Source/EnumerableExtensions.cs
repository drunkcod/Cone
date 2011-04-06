using System.Collections.Generic;

namespace Cone
{
    public static class EnumerableExtenions
    {
        public static int IndexOf<T>(this IEnumerable<T> self, T value) {
            var index = 0;
            foreach(var item in self) {
                if(item.Equals(value))
                    return index; 
                ++index;
            }
            return -1;
        }
    }
}
