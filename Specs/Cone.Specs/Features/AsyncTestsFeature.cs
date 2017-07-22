using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cone.Features
{
	[Feature("async tests")]
	public class AsyncTestsFeature
	{
		public async Task await_a_thing() {
			var r = await Task.FromResult(42);
			Check.That(() => r == 42);
		}

		public async Task await_an_awaiting_awaiter() {
			await Task.WhenAll(
				await_a_thing(),
				Task.Run(() => { }));
		}

		public async Task producer_consumer() {
			var work = new BlockingCollection<int>();

			var producer = Task.Run(() => {
				for(var i = 0; i != 100; ++i)
					work.Add(i);
				work.CompleteAdding();
			});

			var sum = 0;
			var consumer = Task.Run(() => {
				foreach(var item in work.GetConsumingEnumerable())
					sum += item;
			});

			await Task.WhenAll(producer, consumer);
			Check.That(() => sum == 4950);
		}
	}
}
