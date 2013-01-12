using System;

namespace Cone.Core
{
    class TestMethodContext : ITestExecutionContext 
    {
        public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
			var pending = FirstPendingOrDefault(context.Attributes, FirstPendingOrDefault(context.Fixture.FixtureType.AsConeAttributeProvider(), null));
			var isPending = pending != null;

			return (test, result) => {
                try {
					next(test, result);
					if(isPending)
						result.TestFailure(new ExpectationFailedException("Test passed"));
                } catch(Exception ex) {
					if(isPending)
						result.Pending(pending.Reason);
                    else
						result.TestFailure(ex);                        
                }
            };
		}
		
		static IPendingAttribute FirstPendingOrDefault(IConeAttributeProvider attributes, IPendingAttribute defaultValue) {
            return attributes.FirstOrDefault(x => x.IsPending, defaultValue);
        }
    }
}
