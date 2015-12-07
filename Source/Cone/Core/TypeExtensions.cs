using System;

namespace Cone.Core
{
    public static class TypeExtensions
    {
        public static bool Has<T>(this Type type) {
            return type.GetCustomAttributes(typeof(T), true).Length == 1;
        }

        public static bool HasAny(this Type self, params Type[] attributeTypes) {
            var attributes = self.GetCustomAttributes(true);
            return attributes.Any(x => attributeTypes.Any(t => t.IsInstanceOfType(x)));
        }

        public static TResult WithAttributes<TAttribute, TResult>(this Type self, Func<TAttribute[], TResult> found, Func<TResult> notFound) {
            var attributes = self.GetCustomAttributes(typeof(TAttribute), true);
            if (attributes.Length == 0)
                return notFound();
            return found(Array.ConvertAll(attributes, x => (TAttribute)x));
        }

        public static bool TryGetAttribute<TAttribute>(this Type type, out TAttribute value) {
            var attributes = type.GetCustomAttributes(typeof(TAttribute), true);
            if (attributes.Length == 1) {
                value = (TAttribute)attributes[0];
                return true;
            }
            value = default(TAttribute);
            return false;
        }

        public static object New(this Type self) {
            return self.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        public static bool Implements<T>(this Type self) {
            return typeof(T).IsAssignableFrom(self);
        }
    }
}
