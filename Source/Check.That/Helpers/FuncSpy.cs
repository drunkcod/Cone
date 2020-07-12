using System;
using System.Linq;
using System.Linq.Expressions;

namespace CheckThat.Helpers
{
	public class FuncSpy<TResult> : MethodSpy
	{
		public FuncSpy(Func<TResult> inner) : base(inner) { }

		public TResult Invoke() => (TResult)Called();

        public static implicit operator Func<TResult>(FuncSpy<TResult> self) => self.Invoke;
		public static implicit operator FuncSpy<TResult>(Func<TResult> it) => new FuncSpy<TResult>(it);

        public void Then(Action then) => base.Then(then);
	}

    public class FuncSpy<T, TResult> : MethodSpy
    {
        public FuncSpy(Func<T, TResult> inner) : base(inner) { }

		public TResult Invoke(T arg) => (TResult)Called(arg);

        public static implicit operator Func<T, TResult>(FuncSpy<T, TResult> self) => self.Invoke;
		public static implicit operator FuncSpy<T, TResult>(Func<T, TResult> it) => new FuncSpy<T, TResult>(it);

        public void Then(Action<T> then) => base.Then(then);
		public void Check(Expression<Func<T,bool>> first, params Expression<Func<T, bool>>[] rest) =>
			CheckInvocations(new[] { first }.Concat(rest));
    }

    public class FuncSpy<T1, T2, TResult> : MethodSpy
    {
        public FuncSpy(Func<T1, T2, TResult> inner) : base(inner) { }

		public TResult Invoke(T1 arg0, T2 arg1) => (TResult)Called(arg0, arg1);

        public static implicit operator Func<T1, T2, TResult>(FuncSpy<T1, T2, TResult> self) => self.Invoke;
		public static implicit operator FuncSpy<T1, T2, TResult>(Func<T1, T2, TResult> it) => new FuncSpy<T1, T2, TResult>(it);

        public void Then(Action<T1, T2> then) => base.Then(then);
		public void Check(Expression<Func<T1, T2,bool>> first, params Expression<Func<T1, T2, bool>>[] rest) =>
			CheckInvocations(new[] { first }.Concat(rest));
    }

    public class FuncSpy<T1, T2, T3, TResult> : MethodSpy
    {
        public FuncSpy(Func<T1, T2, T3, TResult> inner) : base(inner) { }

		public TResult Invoke(T1 arg0, T2 arg1, T3 arg2) => (TResult)Called(arg0, arg1, arg2);

        public static implicit operator Func<T1, T2, T3, TResult>(FuncSpy<T1, T2, T3, TResult> self) => self.Invoke;
		public static implicit operator FuncSpy<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> it) => new FuncSpy<T1, T2, T3, TResult>(it);

        public void Then(Action<T1, T2, T3> then) => base.Then(then);
		public void Check(Expression<Func<T1, T2, T3,bool>> first, params Expression<Func<T1, T2, T3, bool>>[] rest) =>
			CheckInvocations(new[] { first }.Concat(rest));
    }

    public class FuncSpy<T1, T2, T3, T4, TResult> : MethodSpy
    {
        public FuncSpy(Func<T1, T2, T3, T4, TResult> inner) : base(inner) { }

		public TResult Invoke(T1 arg0, T2 arg1, T3 arg2, T4 arg3) => (TResult)Called(arg0, arg1, arg2, arg3);

        public static implicit operator Func<T1, T2, T3, T4, TResult>(FuncSpy<T1, T2, T3, T4, TResult> self) => self.Invoke;
		public static implicit operator FuncSpy<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> it) => new FuncSpy<T1, T2, T3, T4, TResult>(it);

        public void Then(Action<T1, T2, T3, T4> then) => base.Then(then);
		public void Check(Expression<Func<T1, T2, T3, T4, bool>> first, params Expression<Func<T1, T2, T3, T4, bool>>[] rest) =>
			CheckInvocations(new[] { first }.Concat(rest));
    }
}
