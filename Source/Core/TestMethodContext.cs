using System;

namespace Cone.Core
{
    class TestMethodContext : ITestContext 
    {
        public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
			return (test, result) => {
                try {
					next(test, result);
                } catch(Exception ex) {
                    result.TestFailure(ex);                        
                }
            };
        }
    }
}
