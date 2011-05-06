using System;

namespace Cone.Core
{
    class FixtureAfterContext : ITestContext 
    {
        readonly IConeFixture fixture;

        public FixtureAfterContext(IConeFixture fixture) {
            this.fixture = fixture;
        }

        public Action<ITestResult> Establish(Action<ITestResult> next) {
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
