using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Core
{
	[Describe(typeof(CircularQueue<>))]
	public class CircularQueueSpec
	{
		public void TryEnqueue_fails_when_at_capacity() {
			var q = new CircularQueue<int>(1);
			Assume.That(() => q.TryEnqueue(0));

			Check.That(() => !q.TryEnqueue(1));			
		}

		public void TryEnqueue_fails_if_write_catches_up_to_read() {
			var q = new CircularQueue<int>(2);
			int ignored;
			Assume.That(() => q.TryEnqueue(0));
			Assume.That(() => q.TryDeque(out ignored));

			Assume.That(() => q.TryEnqueue(0));
			Assume.That(() => q.TryEnqueue(1));
			Check.That(() => !q.TryEnqueue(2));
		}

		public void is_first_in_first_out() {
			var q = new CircularQueue<int>(2);

			Assume.That(() => q.TryEnqueue(0));
			Assume.That(() => q.TryEnqueue(1));

			int value;
			Check.That(
				() => q.TryDeque(out value) && value == 0,
				() => q.TryDeque(out value) && value == 1
			);
		}
	}
}
