using System;

namespace Cone.Helpers
{
    public class ActionSpy<T> : MethodSpy
    {
        readonly Action<T> inner;

        public ActionSpy(Action<T> inner) {
            this.inner = inner;
        }

        public static implicit operator Action<T>(ActionSpy<T> self) {
            return x => {
                self.Called();
                self.inner(x);
            };
        }
    }

    public class ActionSpy<T1, T2> : MethodSpy
    {
        readonly Action<T1, T2> inner;

        public ActionSpy(Action<T1, T2> inner) {
            this.inner = inner;
        }

        public static implicit operator Action<T1, T2>(ActionSpy<T1, T2> self) {
            return (arg1, arg2) => {
                self.Called();
                self.inner(arg1, arg2);
            };
        }
    }

    public class ActionSpy<T1, T2, T3> : MethodSpy
    {
        readonly Action<T1, T2, T3> inner;

        public ActionSpy(Action<T1, T2, T3> inner) {
            this.inner = inner;
        }

        public static implicit operator Action<T1, T2, T3>(ActionSpy<T1, T2, T3> self) {
            return (arg1, arg2, arg3) => {
                self.Called();
                self.inner(arg1, arg2, arg3);
            };
        }
    }

    public class ActionSpy<T1, T2, T3, T4> : MethodSpy
    {
        readonly Action<T1, T2, T3, T4> inner;

        public ActionSpy(Action<T1, T2, T3, T4> inner) {
            this.inner = inner;
        }

        public static implicit operator Action<T1, T2, T3, T4>(ActionSpy<T1, T2, T3, T4> self) {
            return (arg1, arg2, arg3, arg4) => {
                self.Called();
                self.inner(arg1, arg2, arg3, arg4);
            };
        }
    }
}
