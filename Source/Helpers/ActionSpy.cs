using System;

namespace Cone.Helpers
{
    public class ActionSpy<T> : MethodSpy
    {
        static void Nop(T _) { }
        
        public ActionSpy() : base(new Action<T>(Nop)) { }
        
        public ActionSpy(Action<T> inner) : base(inner) { }

        public static implicit operator Action<T>(ActionSpy<T> self) {
            return x => self.Called(x);
        }

        public void Then(Action<T> then) { base.Then(then); }
    }

    public class ActionSpy<T1, T2> : MethodSpy
    {
        public ActionSpy(Action<T1, T2> inner) : base(inner) { }

        public static implicit operator Action<T1, T2>(ActionSpy<T1, T2> self) {
            return (arg1, arg2) => self.Called(arg1, arg2);
        }

        public void Then(Action<T1, T2> then) { base.Then(then); }
    }

    public class ActionSpy<T1, T2, T3> : MethodSpy
    {
        public ActionSpy(Action<T1, T2, T3> inner) : base(inner) { }

        public static implicit operator Action<T1, T2, T3>(ActionSpy<T1, T2, T3> self) {
            return (arg1, arg2, arg3) => self.Called(arg1, arg2, arg3);
        }

        public void Then(Action<T1, T2, T3> then) { base.Then(then); }
    }

    public class ActionSpy<T1, T2, T3, T4> : MethodSpy
    {
        public ActionSpy(Action<T1, T2, T3, T4> inner) : base(inner) { }

        public static implicit operator Action<T1, T2, T3, T4>(ActionSpy<T1, T2, T3, T4> self) {
            return (arg1, arg2, arg3, arg4) => self.Called(arg1, arg2, arg3, arg4);
        }

        public void Then(Action<T1, T2, T3, T4> then) { base.Then(then); }
    }
}
