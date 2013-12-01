using System;
using System.Linq;
using Moq;

namespace Cone.Core
{
    [Describe(typeof(TestExecutionContext))]
    public class TestContextContextSpec
    {
        class Fixture {
            public ITestContext Interceptor;
        }

        Mock<ITestContext> TestContext;
        Mock<ITestResult> Result;
        TestExecutionContext ExecutionContext;

        [BeforeEach]
        public void EstablishContext() {
            TestContext = new Mock<ITestContext>();
            Result = new Mock<ITestResult>();
            
			var fixture = new ConeFixture(typeof(Fixture), Enumerable.Empty<string>(), _ => new Fixture { Interceptor = TestContext.Object });
            ExecutionContext = TestExecutionContext.For(fixture);
        }

        public void report_before_failures_and_cleanup() {
            var error = new Exception();
            TestContext.Setup(x => x.Before()).Throws(error);

            ExecutionContext.Establish(null, (_, __) => { })(null, Result.Object);

            Result.Verify(x => x.BeforeFailure(error));
            TestContext.Verify(x => x.After(Result.Object));
        }

        public void doesnt_run_test_when_before_fails() {
            var error = new Exception();
            TestContext.Setup(x => x.Before()).Throws(error);

            ExecutionContext.Establish(null, (_, __) => { throw new InvalidOperationException("should not run test when before fails"); })(null, Result.Object);
        }

        public void report_after_failures() {
            var error = new Exception();
            TestContext.Setup(x => x.After(Result.Object)).Throws(error);

            ExecutionContext.Establish(null, (_, __) => { })(null, Result.Object);

            Result.Verify(x => x.AfterFailure(error));
        }

        public void propagate_exceptions_raised_by_test() {
            TestContextStep raiseException = (_, __) => { throw new Exception(); };
            Check<Exception>.When(() => ExecutionContext.Establish(null, raiseException)(null, Result.Object));
        }
    }
}
