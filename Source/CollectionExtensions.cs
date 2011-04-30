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
    }
}
