using System.Reflection;

namespace Cone.Core
{
    public interface ICallable 
    {
        void Invoke(object obj, object[] parameters);
    }

    public class ConeMethodThunk : ICallable, ICustomAttributeProvider
    {
        readonly MethodInfo method;
        readonly ConeTestNamer namer;

        public ConeMethodThunk(MethodInfo method, ConeTestNamer namer) {
            this.method = method;
            this.namer = namer;
        }

        public void Invoke(object obj, object[] parameters) { method.Invoke(obj, parameters); }

        public string NameFor(object[] parameters) {
            return namer.NameFor(method, parameters);
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(bool inherit) {
            return method.GetCustomAttributes(inherit);
        }

        object[] ICustomAttributeProvider.GetCustomAttributes(System.Type attributeType, bool inherit) {
            return method.GetCustomAttributes(attributeType, inherit);
        }

        bool ICustomAttributeProvider.IsDefined(System.Type attributeType, bool inherit) {
            return method.IsDefined(attributeType, inherit);
        }
    }
}
