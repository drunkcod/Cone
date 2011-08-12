using System;
using System.Reflection;

namespace Cone.Core
{
    class TestMethodContext : ITestContext 
    {
        public Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next) {
			return result => {
                try {
					next(result);
					result.Success();
                } catch(Exception ex) {
                    result.TestFailure(ex);                        
                }
            };
        }
    }
}
