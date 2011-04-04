using System.Globalization;

namespace Cone
{
    public static class ObjectExtensions 
    {
        public static string Inspect(this object obj) {
            return new ObjectInspector(CultureInfo.InvariantCulture).Inspect(obj);
        }
    }
}
