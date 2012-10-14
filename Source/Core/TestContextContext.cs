using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public class TestContextContext : ITestExecutionContext
    {
        readonly Func<object> fixtureProvider; 
        readonly List<FieldInfo> interceptors = new List<FieldInfo>();
 
        TestContextContext(Func<object> fixtureProvider) {
            this.fixtureProvider = fixtureProvider;
        }

        public static TestContextContext For(Type type, Func<object> fixtureProvider) {
            var context = new TestContextContext(fixtureProvider);
            type.GetFields()
                .ForEachIf(
                    x => x.FieldType.Implements<ITestContext>(),
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

        ITestContext GetInterceptor(object fixture, FieldInfo field) {
            return (ITestContext)field.GetValue(fixture);
        }

        void ForEachInterceptor(Action<ITestContext> @do) {
            var fixture = fixtureProvider();
            interceptors.ForEach(x => @do(GetInterceptor(fixture, x)));
        }

        public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
            return (test, result) => {               
                try {
                    if(Before(result))
                        next(test, result);
                } finally {
                    After(result);
                }
            };
        }
    }
}
