using System;
using CheckThat;
using Cone;

namespace CheckThat.Helpers
{
	[Describe(typeof(MethodSpy))]
	public class MethodSpySpec
	{
		public void unwraps_exception() {
			var spy = new ActionSpy(() => { throw new InvalidOperationException();});

			Check.Exception<InvalidOperationException>(() => spy.Invoke());
		}

		public void doesnt_chain_into_null_target() {
			Action<int,int> nothing = null;
			Action<int,int> spy = MethodSpy.On(ref nothing);
			spy(0, 1);		
		}
	}
}