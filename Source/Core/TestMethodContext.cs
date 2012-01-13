using System;
using System.Reflection;

namespace Cone.Core
{
    class TestMethodContext : ITestContext 
    {
        public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
			return (test, result) => {
                try {
					next(test, result);
					result.Success();
                } catch(Exception ex) {
                    result.TestFailure(ex);                        
                }
            };
        }
    }
}
