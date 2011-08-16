using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
    public interface IFixtureContext
    {
        ICustomAttributeProvider Attributes { get; }
        IConeFixture Fixture { get; }
       
    }

    public interface ITestContext 
    {
        Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next);
    }

	class PendingGuardTestContext : ITestContext
	{
		public Action<ITestResult> Establish(IFixtureContext context, Action<ITestResult> next) {
			var pending = FirstPendingOrDefault(context.Attributes, FirstPendingOrDefault(context.Fixture.FixtureType, null));
			return pending == null 
				? next 
				: result => result.Pending(pending.Reason);
        }

        static IPendingAttribute FirstPendingOrDefault(ICustomAttributeProvider attributes, IPendingAttribute defaultValue) {
            return attributes.FirstOrDefault((IPendingAttribute x) => x.IsPending, defaultValue);
        }
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
            readonly ICustomAttributeProvider attributes;
            readonly IConeFixture fixture;

            public FixtureContext(IConeFixture fixture, ICustomAttributeProvider attributes) {
                this.attributes = attributes;
                this.fixture = fixture;
            }

            public ICustomAttributeProvider Attributes { get { return attributes; } }
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

		IEnumerable<ITestContext> GetTestContexts(ICustomAttributeProvider attributes) {
			return attributes.GetCustomAttributes(typeof(ITestContext), true)
				.Cast<ITestContext>();
		}
    }
}
