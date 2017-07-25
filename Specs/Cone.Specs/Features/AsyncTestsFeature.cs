using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
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

		[Row(1)]
		[Row(1000)]
		public static async Task producer_consumer(int numProducers) {
			var work = new BlockingCollection<int>();
			var producers = Enumerable.Range(0, numProducers).Select(x => Task.Factory.StartNew(() => {
				for(var i = 0; i != 100; ++i) { 
					work.Add(i);
				}
			}));

			var sum = 0;
			var consumer = Task.Run(() => {
				foreach(var item in work.GetConsumingEnumerable())
					sum += item;
			});

			await Task.WhenAll(
				Task.WhenAll(producers).ContinueWith(_ => work.CompleteAdding()), 
				consumer);
			Check.That(() => sum == numProducers * 4950);
		}

		[Context("parallel async")]
		public class ParallelAsyncTests
		{
			public Task more_prodsumers() => producer_consumer(1000);
		}

		public void info() {
			Check.That(() => SynchronizationContext.Current == null);
		}

		public Task result() {
			return Task.Run(() => { 
				var a = GetValue(41).Result;
				var b = GetValue(1).Result;
				Check.That(() => a + b == 42);
			});
		}

		async Task<int> GetValue(int value) => await Task.Factory.StartNew(() => value);
 	}
}
