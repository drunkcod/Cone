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

	class PendingGuardTestContext : ITestContext
	{
        readonly IPendingAttribute contextPending;

        public PendingGuardTestContext(ICustomAttributeProvider attributes) {
            this.contextPending = FirstPendingOrDefault(attributes, null);
        }

		public Action<ITestResult> Establish(ICustomAttributeProvider attributes, Action<ITestResult> next) {
			var pending = FirstPendingOrDefault(attributes, contextPending);
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
        readonly List<ITestContext> context;

        public TestExecutor(IConeFixture fixture) {
            this.context = new List<ITestContext> {
                new TestMethodContext(),
				new PendingGuardTestContext(fixture.FixtureType),
                new FixtureBeforeContext(fixture), 
                new FixtureAfterContext(fixture)
            };

            var interceptorContext = InterceptorContext.For(fixture.FixtureType, () => fixture.Fixture);
            if(!interceptorContext.IsEmpty)
                context.Add(interceptorContext);
        }

        public void Run(IConeTest test, ITestResult result) {
            var wrap = HowToWrap(test.Attributes);
            var next = context.Concat(GetTestContexts(test.Attributes)).Aggregate(test.Run, wrap);
			var testContext = test as ITestContext;
			if(testContext != null)
				next = wrap(next, testContext);;
			next(result);
        }

        Func<Action<ITestResult>, ITestContext, Action<ITestResult>> HowToWrap(ICustomAttributeProvider attributes) {
            return (acc, x) => x.Establish(attributes, acc);
        }

		IEnumerable<ITestContext> GetTestContexts(ICustomAttributeProvider attributes) {
			return attributes.GetCustomAttributes(typeof(ITestContext), true)
				.Cast<ITestContext>();
		}
    }
}
