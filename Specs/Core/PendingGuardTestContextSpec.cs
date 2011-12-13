using System;
using System.Reflection;
using Moq;

namespace Cone.Core
{
    [Describe(typeof(PendingGuardTestContext))]
    public class PendingGuardTestContextSpec
    {
        readonly string PendingReason = "Pending Reason";

        Action<ITestResult> NewPendingTestContext(Action<ITestResult> runTest) {
            var pendingGuard = new PendingGuardTestContext();
            var context = new Mock<IFixtureContext>();
            var fixture = new Mock<IConeFixture>();
            var attributes = new Mock<IConeAttributeProvider>();
            attributes.Setup(x => x.GetCustomAttributes(It.IsAny<Type>()))
                .Returns(new object[]{ new PendingAttribute { Reason = PendingReason } });
            
            context.SetupGet(x => x.Attributes).Returns(attributes.Object);
            context.SetupGet(x => x.Fixture).Returns(fixture.Object);

            fixture.SetupGet(x => x.FixtureType).Returns(GetType());
            return pendingGuard.Establish(context.Object, runTest);
        }

        public void sets_pending_reason() {
            var runTest = NewPendingTestContext(x => x.TestFailure(new Exception()));

            var testResult = new Mock<ITestResult>();
            runTest(testResult.Object);

            testResult.Verify(x => x.Pending(PendingReason));
        }

        public void report_failure_when_pending_test_pass() {
            var runTest = NewPendingTestContext(x => x.Success());

            var testResult = new Mock<ITestResult>();
            testResult.SetupGet(x => x.Status).Returns(TestStatus.Success);
            runTest(testResult.Object);

            testResult.Verify(x => x.TestFailure(It.IsAny<ExpectationFailedException>()));
        }
    }
}
