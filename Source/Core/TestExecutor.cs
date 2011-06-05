using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
    public interface ITestContext 
    {
        Action<ITestResult> Establish(ICustomAttributeProvider attributes, Action<ITestResult> next);
    }

	class PendingTestContext : ITestContext
	{
        readonly IPendingAttribute contextPending;

        public PendingTestContext(IPendingAttribute contextPending) {
            this.contextPending = contextPending;
        }

		public Action<ITestResult> Establish(ICustomAttributeProvider attributes, Action<ITestResult> next) {
			var pending = attributes.FirstOrDefault((IPendingAttribute x) => x.IsPending, contextPending);
			return pending == null 
				? next 
				: result => result.Pending(pending.Reason);
        }
	}

    class EstablishVerifyContext : ITestContext 
    {
        readonly Type context;

        public EstablishVerifyContext(Type context) {
            this.context = context;
        }

        public Action<ITestResult> Establish(ICustomAttributeProvider attributes, Action<ITestResult> next) {
            return x => {
                Verify.Context = context;
                next(x);
            };
        }
    }

    public class TestExecutor
    {
        readonly List<ITestContext> context;

        public TestExecutor(IConeFixture fixture) {
            this.context = new List<ITestContext> {
                new TestMethodContext(),
                new EstablishVerifyContext(fixture.FixtureType),
                new FixtureBeforeContext(fixture), 
                new FixtureAfterContext(fixture),
				PendingGuard(fixture.FixtureType)
            };
			this.context.AddRange(GetTestContexts(fixture.FixtureType));

            var interceptorContext = InterceptorContext.For(fixture.FixtureType, () => fixture.Fixture);
            if(!interceptorContext.IsEmpty)
                context.Add(interceptorContext);
        }

        public void Run(IConeTest test, ITestResult result) {
            var next = EstablishContext(test.Attributes, test.Run);
			var context = test as ITestContext;
			if(context != null)
				next = context.Establish(test.Attributes, next);
			next(result);
        }

        Action<ITestResult> EstablishContext(ICustomAttributeProvider attributes, Action<ITestResult> next) {
            return context.Concat(GetTestContexts(attributes))
				.Aggregate(next, (acc, x) => x.Establish(attributes, acc));
        }

		IEnumerable<ITestContext> GetTestContexts(ICustomAttributeProvider attributes) {
			return attributes.GetCustomAttributes(typeof(ITestContext), true)
				.Cast<ITestContext>();
		}

		ITestContext PendingGuard(Type fixtureType) {
			var pending = fixtureType.FirstOrDefault((IPendingAttribute x) => x.IsPending);
			return new PendingTestContext(pending);
		}
    }
}
