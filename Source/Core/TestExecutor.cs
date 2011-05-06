using System;
using System.Collections.Generic;
using System.Linq;

namespace Cone.Core
{
    interface ITestContext 
    {
        Action<ITestResult> Establish(Action<ITestResult> next);
    }

    public class TestExecutor
    {
        readonly Type typeContext;
        readonly List<ITestContext> context;

        public TestExecutor(IConeFixture fixture) {
            this.typeContext = fixture.FixtureType;
            this.context = new List<ITestContext> {
                new TestMethodContext(),
                new FixtureBeforeContext(fixture), 
                new FixtureAfterContext(fixture)
            };

            var interceptorContext = InterceptorContext.For(typeContext, () => fixture.Fixture);
            if(!interceptorContext.IsEmpty)
                context.Add(interceptorContext);
        }

        public void Run(IConeTest test, ITestResult result) {
            var next = EstablishContext(test.Run);            
            next(result);
        }

        Action<ITestResult> EstablishContext(Action<ITestResult> next) {
            Verify.Context = typeContext;
            return context.Aggregate(next, (acc, x) => x.Establish(acc));
        }
    }
}
