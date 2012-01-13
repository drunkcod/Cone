using System;

namespace Cone.Core
{
    class FixtureAfterContext : ITestContext 
    {
        public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
            var fixture = context.Fixture;
            return (test, result) => {
                try {
                    next(test, result);
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
