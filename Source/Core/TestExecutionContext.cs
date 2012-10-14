using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public class TestExecutionContext : ITestExecutionContext
    {
        readonly Func<object> fixtureProvider; 
        readonly List<FieldInfo> testContexts = new List<FieldInfo>();
 
        TestExecutionContext(Func<object> fixtureProvider) {
            this.fixtureProvider = fixtureProvider;
        }

        public static TestExecutionContext For(Type type, Func<object> fixtureProvider) {
            var context = new TestExecutionContext(fixtureProvider);
            type.GetFields()
                .EachWhere(
                    x => x.FieldType.Implements<ITestContext>(),
                    context.testContexts.Add);
            return context;
        }

        public bool IsEmpty { get { return testContexts.Count == 0; } }

        bool Before(ITestResult result) { 
            try {
                EachInterceptor(x => x.Before());
                return true;
            } catch(Exception ex) {
                result.BeforeFailure(ex);
                return false;
            }
        }

        void After(ITestResult result) { 
            try {
                EachInterceptor(x => x.After(result));
            } catch(Exception ex) {
                result.AfterFailure(ex);
            }
        }

        ITestContext GetTestContext(object fixture, FieldInfo field) {
            return (ITestContext)field.GetValue(fixture);
        }

        void EachInterceptor(Action<ITestContext> @do) {
            var fixture = fixtureProvider();
            testContexts.ForEach(x => @do(GetTestContext(fixture, x)));
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
