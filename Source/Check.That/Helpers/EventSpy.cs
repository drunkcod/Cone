using System;
using System.Linq;
using System.Linq.Expressions;

namespace CheckThat.Helpers
{
	public class EventSpy<T> : MethodSpy where T : EventArgs
	{
		public EventSpy() : base(new EventHandler<T>((s, e) => { })) { }
		public EventSpy(EventHandler<T> inner) : base(inner) { }

		public static implicit operator EventHandler<T>(EventSpy<T> self) =>
			self.HandleEvent;

		void HandleEvent(object sender, T args) =>
			Called(sender, args);

		public void Then(EventHandler<T> then) =>
			base.Then(then);

		public void Check(Expression<Func<object, T, bool>> first, params Expression<Func<object, T, bool>>[] rest) =>
			CheckInvocations(new[]{ first }.Concat(rest));	
	}
}
