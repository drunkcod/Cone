using System;

namespace Cone
{
    public static class StringExtensions
    {
        public static int IndexOfDifference(this string self, string other) {
            var equalLengths = self.Length == other.Length;
            int i = 0, end = Math.Min(self.Length, other.Length);
            for(; i != end; ++i)
                if(self[i] != other[i])
                    return i;
            if(equalLengths)
                return -1;
            return end;
        }
    }
}