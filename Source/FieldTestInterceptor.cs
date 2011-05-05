using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone
{
    class FieldTestInterceptor : ITestInterceptor
    {
        readonly Func<object> getFixture; 
        readonly List<FieldInfo> interceptors = new List<FieldInfo>();
 
        public FieldTestInterceptor(Func<object> fixture) {
            this.getFixture = fixture;
        }

        public bool IsEmpty { get { return interceptors.Count == 0; } }

        public void Before() { ForEachInterceptor(x => x.Before()); }

        public void After(ITestResult result) { ForEachInterceptor(x => x.After(result)); }

        public void Add(FieldInfo item) {
            interceptors.Add(item);
        }

        ITestInterceptor GetInterceptor(object fixture, FieldInfo field) {
            return (ITestInterceptor)field.GetValue(fixture);
        }

        void ForEachInterceptor(Action<ITestInterceptor> @do) {
            var fixture = getFixture();
            interceptors.ForEach(x => @do(GetInterceptor(fixture, x)));
        }
    }
}
