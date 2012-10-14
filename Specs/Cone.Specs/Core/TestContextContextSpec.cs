using System;
using Moq;

namespace Cone.Core
{
    [Describe(typeof(TestContextContext))]
    public class TestContextContextSpec
    {
        class Fixture {
            public ITestContext Interceptor;
        }

        Mock<ITestContext> Interceptor;
        Mock<ITestResult> Result;
        TestContextContext Context;

        [BeforeEach]
        public void EstablishContext() {
            Interceptor = new Mock<ITestContext>();
            Result = new Mock<ITestResult>();
            
            var fixture = new Fixture { Interceptor = Interceptor.Object };
            Context = TestContextContext.For(fixture.GetType(), () => fixture);
        }

        public void report_before_failures_and_cleanup() {
            var error = new Exception();
            Interceptor.Setup(x => x.Before()).Throws(error);

            Context.Establish(null, (_, __) => { })(null, Result.Object);

            Result.Verify(x => x.BeforeFailure(error));
            Interceptor.Verify(x => x.After(Result.Object));
        }

        public void doesnt_run_test_when_before_fails() {
            var error = new Exception();
            Interceptor.Setup(x => x.Before()).Throws(error);

            Context.Establish(null, (_, __) => { throw new InvalidOperationException("should not run test when before fails"); })(null, Result.Object);
        }

        public void report_after_failures() {
            var error = new Exception();
            Interceptor.Setup(x => x.After(Result.Object)).Throws(error);

            Context.Establish(null, (_, __) => { })(null, Result.Object);

            Result.Verify(x => x.AfterFailure(error));
        }

        public void propagate_exceptions_raised_by_test() {
            TestContextStep raiseException = (_, __) => { throw new Exception(); };
            Verify.Throws<Exception>.When(() => Context.Establish(null, raiseException)(null, Result.Object));
        }
    }
}
