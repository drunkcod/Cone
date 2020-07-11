using System;
using CheckThat;

namespace Cone.Core
{
	class TestMethodContext : ITestExecutionContext 
	{
		public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
			var pending = FirstPendingOrDefault(
				context.Attributes, FirstPendingOrDefault(context.Fixture.FixtureType.AsConeAttributeProvider(), null));
			return (pending == null)
				? Runnable(next)
				: Pending(next, pending.Reason);
		}

		static TestContextStep Runnable(TestContextStep next) {
			return (test, result) => {
				try {
					result.Begin();
					next(test, result);
				} catch(Exception ex) {
					result.TestFailure(ex);
				}
			};
		}

		static TestContextStep Pending(TestContextStep next, string reason) {
			return (test, result) => {
				try {
					next(test, result);
					result.TestFailure(new CheckFailed("Test passed"));
				} catch {
					result.Pending(reason);
				}
			};
		}
		
		static IPendingAttribute FirstPendingOrDefault(IConeAttributeProvider attributes, IPendingAttribute defaultValue) {
			return attributes.FirstOrDefault(x => x.IsPending, defaultValue);
		}
	}
}
