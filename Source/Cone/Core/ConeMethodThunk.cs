using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
	public class ConeMethodThunk : ICallable, IConeAttributeProvider
	{
		readonly ITestNamer namer;
		readonly IEnumerable<object> attributes; 

		public readonly MethodInfo Method;

		public string Name => Method.Name;

		public ConeMethodThunk(MethodInfo method, IEnumerable<object> attributes, ITestNamer namer) {
			this.namer = namer;
			this.attributes = attributes;
			this.Method = method;
		}

		public void Invoke(object obj, object[] parameters) {
			Method.Invoke(obj, parameters);
		}

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
			return attributes.Where(attributeType.IsInstanceOfType);
		}
	}
}
