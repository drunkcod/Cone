using System;

namespace Cone
{
    public static class TypeExtensions
    {
        public static bool Has<T>(this Type type) {
            return type.GetCustomAttributes(typeof(T), true).Length == 1;
        }

        public static bool TryGetAttribute<TAttribute, TOut>(this Type type, out TOut value) where TAttribute : TOut {
            var attributes = type.GetCustomAttributes(typeof(TAttribute), true);
            if (attributes.Length == 1) {
                value = (TAttribute)attributes[0];
                return true;
            }
            value = default(TOut);
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
