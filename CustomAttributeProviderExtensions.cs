using System.Reflection;

namespace Cone
{
    public static class CustomAttributeProviderExtensions
    {
        public static bool Has<T>(this ICustomAttributeProvider self) {
            return self.GetCustomAttributes(typeof(T), true).Length > 0;
        }
    }
}
