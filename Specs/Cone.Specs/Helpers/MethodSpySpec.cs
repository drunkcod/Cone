using System;

namespace Cone.Helpers
{
	[Describe(typeof(MethodSpy))]
	public class MethodSpySpec
	{
		public void unwraps_exception() {
			var spy = new ActionSpy(() => { throw new InvalidOperationException();});

			Check.Exception<InvalidOperationException>(() => ((Action)spy)());
		}
	}
}