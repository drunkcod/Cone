using System;
using System.Reflection;

namespace Cone.Core
{
    class FixtureBeforeContext : ITestContext
    {
        public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
            var fixture = context.Fixture;
            return (test, result) => {
                try {
                    fixture.Before();
                } catch(Exception ex) {
                    result.BeforeFailure(ex);
                    return;
                }
                next(test, result);
            };
        }
    }
}
