using System;
using System.Reflection;

namespace Cone.Core
{
	public class PendingGuardTestContext : ITestContext
	{
		public Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next) {
			var pending = FirstPendingOrDefault(context.Attributes, FirstPendingOrDefault(context.Fixture.FixtureType, null));
			return pending == null 
				? next 
				: result => ExpectFailure(pending.Reason, next, result);
        }

        static void ExpectFailure(string reason, Action<ITestResult> runTest, ITestResult result) {
            runTest(result);
            if(result.Status == TestStatus.Success)
                result.TestFailure(new ExpectationFailedException("Test passed"));
            else
                result.Pending(reason);
        }

        static IPendingAttribute FirstPendingOrDefault(ICustomAttributeProvider attributes, IPendingAttribute defaultValue) {
            return attributes.FirstOrDefault((IPendingAttribute x) => x.IsPending, defaultValue);
        }
	}
}
