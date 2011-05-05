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
    }

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
