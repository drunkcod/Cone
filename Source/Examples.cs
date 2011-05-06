using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Core;

namespace Cone
{
    public class Examples : IEnumerable<IRowTestData>
    {
        readonly ConeTestNamer testNamer = new ConeTestNamer();
        readonly MethodInfo method;

        readonly List<IRowTestData> rows = new List<IRowTestData>();

        protected Examples(MethodInfo method) {
            this.method = method;
        }

        IEnumerator<IRowTestData> IEnumerable<IRowTestData>.GetEnumerator() { return rows.GetEnumerator(); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return rows.GetEnumerator(); }
    
        protected void AddRow(params object[] parameters) {
            rows.Add(new RowTestData(method, parameters).SetName(testNamer.NameFor(method, parameters)));
        }
    }

    public class Examples<T0> : Examples
    {
        public Examples(Action<T0> action): base(action.Method) { }
        public void Add(T0 arg0) { AddRow(arg0); }
    }

    public class Examples<T0, T1> : Examples
    {
        public Examples(Action<T0, T1> action): base(action.Method) { }
        public void Add(T0 arg0, T1 arg1) { AddRow(arg0, arg1); }
    }

    public class Examples<T0, T1, T2> : Examples
    {
        public Examples(Action<T0, T1, T2> action): base(action.Method) { }
        public void Add(T0 arg0, T1 arg1, T2 arg2) { AddRow(arg0, arg1, arg2); }
    }

    public class Examples<T0, T1, T2, T3> : Examples
    {
        public Examples(Action<T0, T1, T2, T3> action): base(action.Method) { }
        public void Add(T0 arg0, T1 arg1, T2 arg2, T3 arg3) { AddRow(arg0, arg1, arg2, arg3); }
    }

    public class Examples<T0, T1, T2, T3, T4> : Examples
    {
        public Examples(Action<T0, T1, T2, T3, T4> action): base(action.Method) { }
        public void Add(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4) { AddRow(arg0, arg1, arg2, arg3, arg4); }
    }

    public class Examples<T0, T1, T2, T3, T4, T5> : Examples
    {
        public Examples(Action<T0, T1, T2, T3, T4, T5> action): base(action.Method) { }
        public void Add(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) { AddRow(arg0, arg1, arg2, arg3, arg4, arg5); }
    }

    public class Examples<T0, T1, T2, T3, T4, T5, T6> : Examples
    {
        public Examples(Action<T0, T1, T2, T3, T4, T5, T6> action): base(action.Method) { }
        public void Add(T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) { AddRow(arg0, arg1, arg2, arg3, arg4, arg5, arg6); }
    }
}
