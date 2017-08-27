using System;
using System.Threading.Tasks;

namespace Cone.Core
{
	[Describe(typeof(Invokable))]
	public class InvokableSpec
	{
		public void awaited_result() {
			var expected = 42;
			var awaitable = new Func<Task<int>>(() => Task.FromResult(expected));
			Check.That(() => new Invokable(awaitable.Method).Await(awaitable.Target, new object[0]) == (object)expected);
		}
	}
}
