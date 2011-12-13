using System;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
    public static class CustomAttributeProviderExtensions
    {
        public static bool Has<T>(this IConeAttributeProvider self) {
            return self.GetCustomAttributes(typeof(T)).Any();
        }

        public static bool Has<T>(this IConeAttributeProvider self, Action<T[]> with) {
            var values = self.GetCustomAttributes(typeof(T)).ToArray();
            if(values.Length == 0)
                return false;
            with(Array.ConvertAll(values, x => (T)x));
            return true;
        }

        public static T FirstOrDefault<T>(this IConeAttributeProvider self, Func<T, bool> predicate) {
            return FirstOrDefault(self, predicate, default(T));
        }

        public static T FirstOrDefault<T>(this IConeAttributeProvider self, Func<T, bool> predicate, T defaultValue) {
            foreach(var item in self.GetCustomAttributes(typeof(T)).Cast<T>())
                if(predicate(item))
                    return item;
            return defaultValue;
        }

        class ConeCustomAttributeProvider : IConeAttributeProvider
        {
            readonly ICustomAttributeProvider inner;

            public ConeCustomAttributeProvider(ICustomAttributeProvider inner) {
                this.inner = inner;
            }

            public System.Collections.Generic.IEnumerable<object> GetCustomAttributes(Type type) {
                return inner.GetCustomAttributes(type, true);
            }
        }

        public static IConeAttributeProvider AsConeAttributeProvider(this ICustomAttributeProvider self) {
            return new ConeCustomAttributeProvider(self);        
        }
    }
}
