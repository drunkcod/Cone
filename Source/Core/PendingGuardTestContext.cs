using System;
using System.Reflection;

namespace Cone.Core
{
	public class PendingGuardTestContext : ITestExecutionContext
	{
		public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
			var pending = FirstPendingOrDefault(context.Attributes, FirstPendingOrDefault(context.Fixture.FixtureType.AsConeAttributeProvider(), null));
			return pending == null 
				? next 
				: (test, result) => ExpectFailure(pending.Reason, next, test, result);
        }

        static void ExpectFailure(string reason, TestContextStep runTest, IConeTest test, ITestResult result) {
            runTest(test, result);
            if(result.Status == TestStatus.Success)
                result.TestFailure(new ExpectationFailedException("Test passed", Maybe<object>.None, Maybe<object>.None, null));
            else
                result.Pending(reason);
        }

        static IPendingAttribute FirstPendingOrDefault(IConeAttributeProvider attributes, IPendingAttribute defaultValue) {
            return attributes.FirstOrDefault(x => x.IsPending, defaultValue);
        }
	}
}
