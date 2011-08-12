using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
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

        bool Before(ITestResult result) { 
            try {
                ForEachInterceptor(x => x.Before());
                return true;
            } catch(Exception ex) {
                result.BeforeFailure(ex);
                return false;
            }
        }

        void After(ITestResult result) { 
            try {
                ForEachInterceptor(x => x.After(result));
            } catch(Exception ex) {
                result.AfterFailure(ex);
            }
        }

        ITestInterceptor GetInterceptor(object fixture, FieldInfo field) {
            return (ITestInterceptor)field.GetValue(fixture);
        }

        void ForEachInterceptor(Action<ITestInterceptor> @do) {
            var fixture = fixtureProvider();
            interceptors.ForEach(x => @do(GetInterceptor(fixture, x)));
        }

        public Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next) {
            return result => {               
                try {
                    if(Before(result))
                        next(result);
                } finally {
                    After(result);
                }
            };
        }
    }
}
