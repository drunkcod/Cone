using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
    public interface IFixtureContext
    {
        IConeAttributeProvider Attributes { get; }
        IConeFixture Fixture { get; }
    }

    public interface ITestContext 
    {
        Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next);
    }

    public class TestExecutor
    {
        static IEnumerable<ITestContext> ExecutionContext = new ITestContext[] {
            new TestMethodContext(),
		    new PendingGuardTestContext(),
            new FixtureBeforeContext(), 
            new FixtureAfterContext()
        };

        readonly IConeFixture fixture;
        IEnumerable<ITestContext> fixtureContext = new ITestContext[0];

        public TestExecutor(IConeFixture fixture) {
            this.fixture = fixture;
            var interceptorContext = InterceptorContext.For(fixture.FixtureType, () => fixture.Fixture);
            if(!interceptorContext.IsEmpty)
                fixtureContext = new[]{ interceptorContext };
        }

        class FixtureContext : IFixtureContext
        {
            readonly IConeAttributeProvider attributes;
            readonly IConeFixture fixture;

            public FixtureContext(IConeFixture fixture, IConeAttributeProvider attributes) {
                this.attributes = attributes;
                this.fixture = fixture;
            }

            public IConeAttributeProvider Attributes { get { return attributes; } }
            public IConeFixture Fixture { get { return fixture; } }
        }

        public void Run(IConeTest test, ITestResult result) {
            var wrap = CombineEstablish(new FixtureContext(fixture, test.Attributes));
            var next = ExecutionContext
                .Concat(fixtureContext)
                .Concat(GetTestContexts(test.Attributes))
                .Aggregate(test.Run, wrap);
			var testContext = test as ITestContext;
			if(testContext != null)
				next = wrap(next, testContext);;
			next(result);
        }

        Func<Action<ITestResult>, ITestContext, Action<ITestResult>> CombineEstablish(IFixtureContext context) {
            return (acc, x) => x.Establish(context, acc);
        }

		IEnumerable<ITestContext> GetTestContexts(IConeAttributeProvider attributes) {
			return attributes.GetCustomAttributes(typeof(ITestContext))
				.Cast<ITestContext>();
		}
    }
}
