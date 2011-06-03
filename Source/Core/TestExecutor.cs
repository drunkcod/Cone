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

	class PendingMethodContext : ITestContext
	{
		public Action<ITestResult> Establish(ICustomAttributeProvider attributes, Action<ITestResult> next) {
			var pending = attributes.FirstOrDefault((IPendingAttribute x) => x.IsPending);
			return pending == null 
				? next 
				: result => result.Pending(pending.Reason);
        }
	}

	class PendingFixtureContext : ITestContext
	{
		public string Reason;

		public Action<ITestResult> Establish(ICustomAttributeProvider attributes, Action<ITestResult> next) {
			return result => result.Pending(Reason);
        }
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
                new FixtureAfterContext(fixture),
				PendingGuard(fixture.FixtureType)
            };
			this.context.AddRange(GetTestContexts(fixture.FixtureType));

            var interceptorContext = InterceptorContext.For(typeContext, () => fixture.Fixture);
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
            Verify.Context = typeContext;
            return context.Concat(GetTestContexts(attributes))
				.Aggregate(next, (acc, x) => x.Establish(attributes, acc));
        }

		IEnumerable<ITestContext> GetTestContexts(ICustomAttributeProvider attributes) {
			return attributes.GetCustomAttributes(typeof(ITestContext), true)
				.Cast<ITestContext>();
		}

		ITestContext PendingGuard(Type fixtureType) {
			var pending = fixtureType.FirstOrDefault((IPendingAttribute x) => x.IsPending);
			if(pending != null)
				return new PendingFixtureContext { Reason = pending.Reason };
			return new PendingMethodContext();
		}
    }
}
