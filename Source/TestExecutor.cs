using System;
using System.Collections.Generic;
using System.Linq;
using Cone.Core;

namespace Cone
{
    interface ITestContext 
    {
        Action<ITestResult> Establish(Action<ITestResult> next);
    }

    public class TestExecutor
    {
        readonly Type typeContext;
        readonly List<ITestContext> context = new List<ITestContext>();

        public TestExecutor(IConeFixture fixture) {
            this.typeContext = fixture.FixtureType;
            
            context.Add(new ConeFixtureContext(fixture));

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
