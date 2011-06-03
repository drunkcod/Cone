using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
    interface ITestContext 
    {
        Action<ITestResult> Establish(ICustomAttributeProvider attributes, Action<ITestResult> next);
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
            };
			this.context.AddRange(GetTestContexts(fixture.FixtureType));

            var interceptorContext = InterceptorContext.For(typeContext, () => fixture.Fixture);
            if(!interceptorContext.IsEmpty)
                context.Add(interceptorContext);
        }

        public void Run(IConeTest test, ITestResult result) {
            var next = EstablishContext(test.Attributes, test.Run);
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
    }
}
