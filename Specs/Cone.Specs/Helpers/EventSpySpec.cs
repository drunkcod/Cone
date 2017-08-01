using System;
using System.Collections.Generic;
using System.Linq;

namespace Cone.Helpers
{
	[Describe(typeof(EventSpy<>))]
	public class EventSpySpec
	{
		class TestTarget
		{
			public event EventHandler<EventArgs> OnFoo;

			public void Foo() =>
				OnFoo?.Invoke(this, EventArgs.Empty);
		}

		public void records_number_of_invocations() {
			var target = new TestTarget();
			var fooSpy = new EventSpy<EventArgs>();

			target.OnFoo += fooSpy;
			target.Foo();
			Check.That(() => fooSpy.HasBeenCalled);
			var calls = new List<Tuple<object,EventArgs>>();
			fooSpy.Then((s, e) => calls.Add(Tuple.Create(s, e)));
			Check.That(
				() => calls.Count == 1,
				() => calls[0].Item1 == target,
				() => calls[0].Item2 == EventArgs.Empty);
		}
	}
}
