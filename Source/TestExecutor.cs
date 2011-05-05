using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone
{
    public class TestExecutor
    {
        readonly Type context;
        readonly List<ITestInterceptor> interceptors = new List<ITestInterceptor>();

        public TestExecutor(IConeFixture fixture) {
            this.context = fixture.FixtureType;

            var interceptorFields = new FieldTestInterceptor(() => fixture.Fixture);
            context.GetFields()
                .ForEachIf(
                    x => x.FieldType.Implements<ITestInterceptor>(),
                    interceptorFields.Add);
            if(!interceptorFields.IsEmpty)
                interceptors.Add(interceptorFields);
            interceptors.Add(fixture);
        }

        public void Run(IConeTest test, ITestResult testResult) {
            EstablishContext();
            Maybe(Before, () => {
                    Maybe(() => test.Run(testResult), 
                        testResult.Success, 
                        testResult.TestFailure);
                }, testResult.BeforeFailure);
            Maybe(() => After(testResult), () => { }, testResult.AfterFailure);
        }

        void EstablishContext() {
            Verify.Context = context;
        }

        void Before() {
            interceptors.ForEach(x => x.Before());
        }

        void After(ITestResult result) { 
            interceptors.BackwardsEach(x => x.After(result));
        }

        static void Maybe(Action action, Action then, Action<Exception> fail) {
            try {
                action();
                then();
            } catch (TargetInvocationException ex) {
                fail(ex.InnerException);
            } catch (Exception ex) {
                fail(ex);
            }
        }
    }
}
