using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone.Expectations;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Core
{
	[Describe(typeof(MethodExpectProviderLookup))]
	public class MethodExpectProviderLookupSpec
	{
		public void MyMethod<T>(T value) { }

		class NullMethodExpectProvider : IMethodExpectProvider {
			public IEnumerable<MethodInfo> GetSupportedMethods() { return new MethodInfo[0]; }

			public IExpect GetExpectation(Expression body, MethodInfo method, object target, object[] args) {
                return null;
			}
		}

		public void finds_generic_provider_if_available() {
			var lookup = new MethodExpectProviderLookup();
			var provider = new NullMethodExpectProvider();
            var method = GetType().GetMethod("MyMethod");
			lookup.Insert(method, provider);

			IMethodExpectProvider foundProvider = null;
			Check.That(() => lookup.TryGetExpectProvider(method.MakeGenericMethod(typeof(int)), out foundProvider));
            Check.That(() => foundProvider == provider);

		}
	}
}
