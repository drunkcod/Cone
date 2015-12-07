using System.Collections.Generic;
using System.Reflection;

namespace Cone.Expectations
{
	public class MethodExpectProviderLookup 
	{
		readonly Dictionary<MethodInfo, IMethodExpectProvider> lookup = new Dictionary<MethodInfo, IMethodExpectProvider>();

		public void Insert(MethodInfo method, IMethodExpectProvider provider) {
			lookup[method] = provider;
		}

		public bool TryGetExpectProvider(MethodInfo method, out IMethodExpectProvider provider) {
			var foundExact = lookup.TryGetValue(method, out provider);
			if(!foundExact && method.IsGenericMethod)
				return lookup.TryGetValue(method.GetGenericMethodDefinition(), out provider);
			return foundExact;
		}
	}
}
