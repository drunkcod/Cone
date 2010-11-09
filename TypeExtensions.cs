using System;

namespace Cone
{
    public static class TypeExtensions
    {
        public static bool Has<T>(this Type type) {
            return type.GetCustomAttributes(typeof(T), true).Length == 1;
        }

        public static bool TryGetAttribute<T>(this Type type, out T value) {
            var attributes = type.GetCustomAttributes(typeof(T), true);
            if (attributes.Length == 1) {
                value = (T)attributes[0];
                return true;
            }
            value = default(T);
            return false;
        }
    }
}
