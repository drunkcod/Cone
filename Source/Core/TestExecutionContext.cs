using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Cone.Core
{
    public class TestExecutionContext : ITestExecutionContext
    {
        readonly IConeFixture fixture; 
        readonly List<FieldInfo> testContexts = new List<FieldInfo>();
 
        TestExecutionContext(IConeFixture fixture) {
            this.fixture = fixture;
        }

        public static TestExecutionContext For(IConeFixture fixture) {
            var context = new TestExecutionContext(fixture);
            fixture.FixtureType.GetFields()
                .ForEachWhere(
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

        ITestContext GetTestContext(FieldInfo field) {
            return (ITestContext)fixture.GetValue(field);
        }

        void EachInterceptor(Action<ITestContext> @do) {
            testContexts.ForEach(x => @do(GetTestContext(x)));
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
