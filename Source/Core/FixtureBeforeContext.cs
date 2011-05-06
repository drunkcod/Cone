using System;

namespace Cone.Core
{
    class FixtureBeforeContext : ITestContext
    {
        readonly IConeFixture fixture;

        public FixtureBeforeContext(IConeFixture fixture) {
            this.fixture = fixture;
        }

        public Action<ITestResult> Establish(Action<ITestResult> next) {
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
