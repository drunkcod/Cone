using System;
using System.Collections.Generic;

namespace Cone
{
    static class CollectionExtensions
    {
        public static TOutput[] ConvertAll<TInput,TOutput>(this IList<TInput> self, Converter<TInput, TOutput> converter) {
            var result = new TOutput[self.Count];
            for(var i = 0; i != result.Length; ++i)
                result[i] = converter(self[i]);
            return result;
        }

        public static void ForEach<T>(this T[] self, Action<T> @do) {
            for(var i = 0; i != self.Length; ++i)
                @do(self[i]);
        }

        public static void ForEach<T>(this IEnumerable<T> self, Action<T> @do) {
            foreach(var item in self)
                @do(item);
        }

        public static void ForEach<T>(this IEnumerable<T> self, Action<int, T> @do) {
            using(var items = self.GetEnumerator())
                for(var i = 0; items.MoveNext(); ++i)
                    @do(i, items.Current);
        }
    }
}
