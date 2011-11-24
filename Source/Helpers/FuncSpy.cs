using System;

namespace Cone.Helpers
{
    public class FuncSpy<T, TResult> : MethodSpy
    {
        public FuncSpy(Func<T, TResult> inner) : base(inner) { }

        public static implicit operator Func<T, TResult>(FuncSpy<T, TResult> self) {
            return x => (TResult)self.Called(x);
        }

        public void Then(Action<T> then) { base.Then(then); }
    }

    public class FuncSpy<T1, T2, TResult> : MethodSpy
    {
        public FuncSpy(Func<T1, T2, TResult> inner) : base(inner) { }

        public static implicit operator Func<T1, T2, TResult>(FuncSpy<T1, T2, TResult> self) {
            return (arg1, arg2) => (TResult)self.Called(arg1, arg2);
        }

        public void Then(Action<T1, T2> then) { base.Then(then); }
    }

    public class FuncSpy<T1, T2, T3, TResult> : MethodSpy
    {
        public FuncSpy(Func<T1, T2, T3, TResult> inner) : base(inner) { }

        public static implicit operator Func<T1, T2, T3, TResult>(FuncSpy<T1, T2, T3, TResult> self) {
            return (arg1, arg2, arg3) => (TResult)self.Called(arg1, arg2, arg3);
        }

        public void Then(Action<T1, T2, T3> then) { base.Then(then); }
    }

    public class FuncSpy<T1, T2, T3, T4, TResult> : MethodSpy
    {
        public FuncSpy(Func<T1, T2, T3, T4, TResult> inner) : base(inner) { }

        public static implicit operator Func<T1, T2, T3, T4, TResult>(FuncSpy<T1, T2, T3, T4, TResult> self) {
            return (arg1, arg2, arg3, arg4) => (TResult)self.Called(arg1, arg2, arg3, arg4);
        }

        public void Then(Action<T1, T2, T3, T4> then) { base.Then(then); }
    }
}
