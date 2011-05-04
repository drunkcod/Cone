using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone
{
    class FieldTestInterceptor : ITestInterceptor
    {
        readonly object fixture; 
        readonly List<FieldInfo> interceptors = new List<FieldInfo>();
 
        public FieldTestInterceptor(object fixture) {
            this.fixture = fixture;
        }

        public bool IsEmpty { get { return interceptors.Count == 0; } }

        public void Before() { ForEachInterceptor(x => x.Before()); }

        public void After(ITestResult result) { ForEachInterceptor(x => x.After(result)); }

        public void Add(FieldInfo item) {
            interceptors.Add(item);
        }

        ITestInterceptor GetInterceptor(FieldInfo field) {
            return (ITestInterceptor)field.GetValue(fixture);
        }

        void ForEachInterceptor(Action<ITestInterceptor> @do) {
            interceptors.ForEach(x => @do(GetInterceptor(x)));
        }
    }
}
