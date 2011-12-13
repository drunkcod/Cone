using System;
using Moq;

namespace Cone.Core
{
    [Describe(typeof(InterceptorContext))]
    public class InterceptorContextSpec
    {
        class Fixture {
            public ITestInterceptor Interceptor;
        }

        Mock<ITestInterceptor> Interceptor;
        Mock<ITestResult> Result;
        InterceptorContext Context;

        [BeforeEach]
        public void EstablishContext() {
            Interceptor = new Mock<ITestInterceptor>();
            Result = new Mock<ITestResult>();
            
            var fixture = new Fixture { Interceptor = Interceptor.Object };
            Context = InterceptorContext.For(fixture.GetType(), () => fixture);
        }

        public void report_before_failures_and_cleanup() {
            var error = new Exception();
            Interceptor.Setup(x => x.Before()).Throws(error);

            Context.Establish(null, _ => { })(Result.Object);

            Result.Verify(x => x.BeforeFailure(error));
            Interceptor.Verify(x => x.After(Result.Object));
        }

        public void doesnt_run_test_when_before_fails() {
            var error = new Exception();
            Interceptor.Setup(x => x.Before()).Throws(error);

            Context.Establish(null, _ => { throw new InvalidOperationException("should not run test when before fails"); })(Result.Object);
        }

        public void report_after_failures() {
            var error = new Exception();
            Interceptor.Setup(x => x.After(Result.Object)).Throws(error);

            Context.Establish(null, _ => { })(Result.Object);

            Result.Verify(x => x.AfterFailure(error));
        }

        public void propagate_exceptions_raised_by_test() {
            Action<ITestResult> raiseException = _ => { throw new Exception(); };
            Verify.Throws<Exception>.When(() => Context.Establish(null, raiseException)(Result.Object));
        }
    }
}
