using System;

namespace Cone.Helpers
{
    public class FuncSpy<T, TResult> : MethodSpy
    {
        readonly Func<T, TResult> inner;

        public FuncSpy(Func<T, TResult> inner) {
            this.inner = inner;
        }

        public static implicit operator Func<T, TResult>(FuncSpy<T, TResult> self) {
            return x => {
                self.Called();
                return self.inner(x);
            };
        }
    }

    public class FuncSpy<T1, T2, TResult> : MethodSpy
    {
        readonly Func<T1, T2, TResult> inner;

        public FuncSpy(Func<T1, T2, TResult> inner) {
            this.inner = inner;
        }

        public static implicit operator Func<T1, T2, TResult>(FuncSpy<T1, T2, TResult> self) {
            return (arg1, arg2) => {
                self.Called();
                return self.inner(arg1, arg2);
            };
        }
    }

    public class FuncSpy<T1, T2, T3, TResult> : MethodSpy
    {
        readonly Func<T1, T2, T3, TResult> inner;

        public FuncSpy(Func<T1, T2, T3, TResult> inner) {
            this.inner = inner;
        }

        public static implicit operator Func<T1, T2, T3, TResult>(FuncSpy<T1, T2, T3, TResult> self) {
            return (arg1, arg2, arg3) => {
                self.Called();
                return self.inner(arg1, arg2, arg3);
            };
        }
    }

    public class FuncSpy<T1, T2, T3, T4, TResult> : MethodSpy
    {
        readonly Func<T1, T2, T3, T4, TResult> inner;

        public FuncSpy(Func<T1, T2, T3, T4, TResult> inner) {
            this.inner = inner;
        }

        public static implicit operator Func<T1, T2, T3, T4, TResult>(FuncSpy<T1, T2, T3, T4, TResult> self) {
            return (arg1, arg2, arg3, arg4) => {
                self.Called();
                return self.inner(arg1, arg2, arg3, arg4);
            };
        }
    }
}
