using System;
using System.Reflection;

namespace Cone.Core
{
    class FixtureBeforeContext : ITestContext
    {
        public Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next) {
            var fixture = context.Fixture;
            return result => {
                try {
                    fixture.Before();
                } catch(Exception ex) {
                    result.BeforeFailure(ex);
                    return;
                }
                next(result);
            };
        }
    }
}
