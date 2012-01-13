using System;
using System.Reflection;

namespace Cone.Core
{
	public class PendingGuardTestContext : ITestContext
	{
		public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
			var pending = FirstPendingOrDefault(context.Attributes, FirstPendingOrDefault(context.Fixture.FixtureType.AsConeAttributeProvider(), null));
			return pending == null 
				? next 
				: new TestContextStep((test, result) => ExpectFailure(pending.Reason, next, test, result));
        }

        static void ExpectFailure(string reason, TestContextStep runTest, IConeTest test, ITestResult result) {
            runTest(test, result);
            if(result.Status == TestStatus.Success)
                result.TestFailure(new ExpectationFailedException("Test passed"));
            else
                result.Pending(reason);
        }

        static IPendingAttribute FirstPendingOrDefault(IConeAttributeProvider attributes, IPendingAttribute defaultValue) {
            return attributes.FirstOrDefault((IPendingAttribute x) => x.IsPending, defaultValue);
        }
	}
}
