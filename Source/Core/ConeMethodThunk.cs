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
        public readonly MethodInfo Method;

        readonly ConeTestNamer namer;

        public ConeMethodThunk(MethodInfo method, ConeTestNamer namer) {
            this.Method = method;
            this.namer = namer;
        }

        public void Invoke(object obj, object[] parameters) { Method.Invoke(obj, parameters); }

		public string GetHeading() {
			return namer.NameFor(Method);
		}

        public string NameFor(object[] parameters) {
            return namer.NameFor(Method, parameters);
        }

		public ITestName TestNameFor(string context, object[] parameters) {
			return namer.TestNameFor(context, Method, parameters);
		}

        public IEnumerable<object> GetCustomAttributes(Type attributeType) {
            return Method.GetCustomAttributes(attributeType, true);
        }
    }
}
