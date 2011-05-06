using System;

namespace Cone.Core
{
    class TestMethodContext : ITestContext 
    {
        public Action<ITestResult> Establish(Action<ITestResult> next) {
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
