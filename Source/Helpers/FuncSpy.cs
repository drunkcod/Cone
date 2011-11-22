using System;

namespace Cone.Helpers
{
    public static class FuncSpy
    {
        public static FuncSpy<T, TResult> On<T, TResult>(ref Func<T, TResult> target, Func<T, TResult> inner) {
            var spy = new FuncSpy<T,TResult>(inner);
            target = spy;
            return spy;
        }

        public static FuncSpy<T, TResult> For<T, TResult>(Func<T, TResult> inner) {
            return new FuncSpy<T,TResult>(inner);
        }

        public static FuncSpy<T1,T2, TResult> On<T1, T2, TResult>(ref Func<T1, T2, TResult> target, Func<T1, T2, TResult> inner) {
            var spy = new FuncSpy<T1,T2,TResult>(inner);
            target = spy;
            return spy;
        }

        public static FuncSpy<T1,T2, TResult> For<T1, T2, TResult>(Func<T1, T2, TResult> inner) {
            return new FuncSpy<T1,T2,TResult>(inner);
        }
    }

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

    public class FuncSpy<T1,T2, TResult> : MethodSpy
    {
        readonly Func<T1, T2, TResult> inner;

        public FuncSpy(Func<T1, T2, TResult> inner) {
            this.inner = inner;
        }

        public static implicit operator Func<T1, T2, TResult>(FuncSpy<T1,T2, TResult> self) {
            return (t1, t2) => {
                self.Called();
                return self.inner(t1, t2);
            };
        }
    }
}
