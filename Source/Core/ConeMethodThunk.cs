using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface ICallable 
    {
        void Invoke(object obj, object[] parameters);
    }

    public class ConeMethodThunk : ICallable, IConeAttributeProvider
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

        IEnumerable<object> IConeAttributeProvider.GetCustomAttributes(Type attributeType) {
            return method.GetCustomAttributes(attributeType, true);
        }
    }
}
