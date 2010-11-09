using System;
using System.Reflection;

namespace Cone
{
    public class TestExecutor
    {
        readonly IConeFixture fixture;

        public TestExecutor(IConeFixture fixture) {
            this.fixture = fixture;
        }

        public void Run(IConeTest test, ITestResult testResult) {
            Maybe(fixture.Before, 
                () => {
                    Maybe(() => test.Run(testResult), testResult.Success, testResult.TestFailure);
                    Maybe(() => fixture.After(testResult), () => { }, testResult.AfterFailure);               
                }, testResult.BeforeFailure);
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
