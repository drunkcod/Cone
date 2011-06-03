using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone.Expectations;

namespace Cone.Core
{
	[Describe(typeof(MethodExpectProviderLookup))]
	public class MethodExpectProviderLookupSpec
	{
		public void MyMethod<T>(T value) { }

		class NullMethodExpectProvider : IMethodExpectProvider {
			public IEnumerable<System.Reflection.MethodInfo> GetSupportedMethods() {
				throw new NotImplementedException();
			}

			public IExpect GetExpectation(System.Linq.Expressions.Expression body, System.Reflection.MethodInfo method, object target, object[] args) {
				throw new NotImplementedException();
			}
		}

		public void finds_generic_provider_if_available() {
			var lookup = new MethodExpectProviderLookup();
			var provider = new NullMethodExpectProvider();
			lookup.Insert(GetType().GetMethod("MyMethod"), provider);

			IMethodExpectProvider foundProvider;
			Verify.That(() => 
				lookup.TryGetExpectProvider(GetType().GetMethod("MyMethod").MakeGenericMethod(typeof(int)), out foundProvider) 
				&& foundProvider == provider);

		}
	}
}
