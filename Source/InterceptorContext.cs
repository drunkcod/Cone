using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone
{
    public class InterceptorContext : ITestContext
    {
        readonly Func<object> fixtureProvider; 
        readonly List<FieldInfo> interceptors = new List<FieldInfo>();
 
        InterceptorContext(Func<object> fixtureProvider) {
            this.fixtureProvider = fixtureProvider;
        }

        public static InterceptorContext For(Type type, Func<object> fixtureProvider) {
            var context = new InterceptorContext(fixtureProvider);
            type.GetFields()
                .ForEachIf(
                    x => x.FieldType.Implements<ITestInterceptor>(),
                    context.interceptors.Add);
            return context;
        }

        public bool IsEmpty { get { return interceptors.Count == 0; } }

        void Before() { ForEachInterceptor(x => x.Before()); }

        void After(ITestResult result) { ForEachInterceptor(x => x.After(result)); }

        ITestInterceptor GetInterceptor(object fixture, FieldInfo field) {
            return (ITestInterceptor)field.GetValue(fixture);
        }

        void ForEachInterceptor(Action<ITestInterceptor> @do) {
            var fixture = fixtureProvider();
            interceptors.ForEach(x => @do(GetInterceptor(fixture, x)));
        }

        public Action<ITestResult> Establish(Action<ITestResult> next) {
            return result => {
                try {
                    Before();
                    next(result);
                } catch(Exception ex) {
                    result.BeforeFailure(ex);
                } finally {
                    try {
                        After(result);
                    } catch(Exception ex) {
                        result.AfterFailure(ex);
                    }
                }
            };
        }
    }
}
