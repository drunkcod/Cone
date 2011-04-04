using System.Reflection;
using System.Reflection.Emit;
using System;

namespace Cone
{
    public class ConeMethodThunk : ICustomAttributeProvider
    {
        readonly MethodInfo method;
        readonly ConeTestNamer namer;

        public ConeMethodThunk(MethodInfo method, ConeTestNamer namer) {
            this.method = method;
            this.namer = namer;
        }

        public void Invoke(object fixture, object[] parameters) { method.Invoke(fixture, parameters); }

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
