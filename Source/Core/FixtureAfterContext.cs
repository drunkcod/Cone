using System;

namespace Cone.Core
{
    class FixtureAfterContext : ITestContext 
    {
        public Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next) {
            var fixture = context.Fixture;
            return result => {
                try {
                    next(result);
                } finally {
                    try {
                        fixture.After(result);
                    } catch (Exception ex) {
                        result.AfterFailure(ex);
                    }
                }
            };
        }
    }}
