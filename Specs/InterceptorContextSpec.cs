using System;
using Moq;

namespace Cone
{
    [Describe(typeof(InterceptorContext))]
    public class InterceptorContextSpec
    {
        class FakeInterceptor : ITestInterceptor
        {
            public Action BeforeAction = () => { };
            public Action<ITestResult> AfterAction = _ => { };

            public void Before() { BeforeAction(); }

            public void After(ITestResult result) { AfterAction(result); }
        }

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

            Context.Establish(_ => { })(Result.Object);

            Result.Verify(x => x.BeforeFailure(error));
            Interceptor.Verify(x => x.After(Result.Object));
        }

        public void report_after_failures() {
            var error = new Exception();
            Interceptor.Setup(x => x.After(Result.Object)).Throws(error);

            Context.Establish(_ => { })(Result.Object);

            Result.Verify(x => x.AfterFailure(error));
        }
    }
}
