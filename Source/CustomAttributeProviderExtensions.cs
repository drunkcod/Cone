using System;
using System.Reflection;

namespace Cone
{
    public static class CustomAttributeProviderExtensions
    {
        public static bool Has<T>(this ICustomAttributeProvider self) {
            return self.IsDefined(typeof(T), true);
        }

        public static bool Has<T>(this ICustomAttributeProvider self, Action<T[]> with) {
            var values = self.GetCustomAttributes(typeof(T), true);
            if(values.Length == 0)
                return false;
            with(Array.ConvertAll(values, x => (T)x));
            return true;
        }

        public static T FirstOrDefault<T>(this ICustomAttributeProvider self, Func<T, bool> predicate) {
            return FirstOrDefault(self, predicate, default(T));
        }

        public static T FirstOrDefault<T>(this ICustomAttributeProvider self, Func<T, bool> predicate, T defaultValue) {
            var values = Array.ConvertAll(self.GetCustomAttributes(typeof(T), true), x => (T)x);
            for(var i = 0; i != values.Length; ++i) {
                var item = values[i];
                if(predicate(item))
                    return item;
            }
            return defaultValue;
        }

    }
}
