using System;
using System.Linq;
using System.Linq.Expressions;

namespace Cone.Helpers
{
	public class ActionSpy : MethodSpy
	{
		public ActionSpy() : this(null) { }
		public ActionSpy(Action inner) : base(inner) { }

		public void Invoke() => Called();

		public static implicit operator Action(ActionSpy self) => self.Invoke;
		public static implicit operator ActionSpy(Action it) => new ActionSpy(it);
	}

	public class ActionSpy<T> : MethodSpy
	{
		public ActionSpy() : this(null) { }
		public ActionSpy(Action<T> inner) : base(inner) { }

		public void Invoke(T arg0) => Called(arg0);

		public static implicit operator Action<T>(ActionSpy<T> self) => self.Invoke;
		public static implicit operator ActionSpy<T>(Action<T> it) => new ActionSpy<T>(it);

		public void Then(Action<T> then) => base.Then(then);
		public void Check(Expression<Func<T, bool>> first, params Expression<Func<T, bool>>[] rest) =>
			CheckInvocations(new[]{ first}.Concat(rest));
	}

	public class ActionSpy<T1, T2> : MethodSpy
	{
		public ActionSpy() : this(null) { }
		public ActionSpy(Action<T1, T2> inner) : base(inner) { }

		public void Invoke(T1 arg0, T2 arg1) => Called(arg0, arg1);

		public static implicit operator Action<T1, T2>(ActionSpy<T1, T2> self) => self.Invoke;
		public static implicit operator ActionSpy<T1, T2>(Action<T1, T2> it) => new ActionSpy<T1, T2>(it);

		public void Then(Action<T1, T2> then) => base.Then(then);
		public void Check(Expression<Func<T1, T2, bool>> first, params Expression<Func<T1, T2, bool>>[] rest) =>
			CheckInvocations(new[]{ first }.Concat(rest));
	}

	public class ActionSpy<T1, T2, T3> : MethodSpy
	{
		public ActionSpy() : this(null) { }
		public ActionSpy(Action<T1, T2, T3> inner) : base(inner) { }

		public void Invoke(T1 arg0, T2 arg1, T3 arg2) => Called(arg0, arg1, arg2);

		public static implicit operator Action<T1, T2, T3>(ActionSpy<T1, T2, T3> self) => self.Invoke;
		public static implicit operator ActionSpy<T1, T2, T3>(Action<T1, T2, T3> it) => new ActionSpy<T1, T2, T3>(it);

		public void Then(Action<T1, T2, T3> then) => base.Then(then);
		public void Check(Expression<Func<T1,T2,T3,bool>> first, params Expression<Func<T1, T2, T3, bool>>[] rest) =>
			CheckInvocations(new [] { first}.Concat(rest));
	}

	public class ActionSpy<T1, T2, T3, T4> : MethodSpy
	{
		public ActionSpy() : this(null) { }
		public ActionSpy(Action<T1, T2, T3, T4> inner) : base(inner) { }

		public void Invoke(T1 arg0, T2 arg1, T3 arg2, T4 arg3) => Called(arg0, arg1, arg2, arg3);

		public static implicit operator Action<T1, T2, T3, T4>(ActionSpy<T1, T2, T3, T4> self) => self.Invoke;
		public static implicit operator ActionSpy<T1, T2, T3, T4>(Action<T1, T2, T3, T4> it) => new ActionSpy<T1, T2, T3, T4>(it);

		public void Then(Action<T1, T2, T3, T4> then) => base.Then(then);
		public void Check(Expression<Func<T1,T2,T3, T4,bool>> first, params Expression<Func<T1, T2, T3, T4, bool>>[] rest) =>
			CheckInvocations(new [] { first}.Concat(rest));
	}
}
