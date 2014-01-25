using System;
using System.Collections.Generic;
using System.Linq;

namespace Cone.Core
{
    public interface IFixtureContext
    {
        IConeAttributeProvider Attributes { get; }
        IConeFixture Fixture { get; }
    }

    public delegate void TestContextStep(IConeTest test, ITestResult result);

    public interface ITestExecutionContext 
    {
        TestContextStep Establish(IFixtureContext context, TestContextStep next);
    }

	public interface ITestExecutor
	{
		void Run(IConeTest test, ITestResult result);
		void Initialize();
		void Relase();
	}

	public class DryRunTestExecutor : ITestExecutor
	{
		public void Run(IConeTest test, ITestResult result) {
			result.Success();
		}

		public void Initialize() { }
		public void Relase() { }
	}

    public class TestExecutor : ITestExecutor
    {
        static readonly IEnumerable<ITestExecutionContext> ExecutionContext = new ITestExecutionContext[] {
            new TestMethodContext(),
            new FixtureBeforeContext(), 
            new FixtureAfterContext()
        };

        readonly IConeFixture fixture;
        readonly IEnumerable<ITestExecutionContext> fixtureContext = new ITestExecutionContext[0];

        public TestExecutor(IConeFixture fixture) {
            this.fixture = fixture;
            var interceptorContext = TestExecutionContext.For(fixture);
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

        class NullContext : ITestExecutionContext 
        {
            public TestContextStep Establish(IFixtureContext context, TestContextStep next) {
                return next;
            }
        }

        public void Run(IConeTest test, ITestResult result) {
            Run(test, result, new NullContext());
        }
        
        public void Run(IConeTest test, ITestResult result, ITestExecutionContext context) {
            var wrap = CombineEstablish(new FixtureContext(fixture, test.Attributes));
            var next = ExecutionContext
                .Concat(fixtureContext)
                .Concat(GetTestContexts(test.Attributes))
                .Aggregate((t, r) => t.Run(r), wrap);
			var testContext = test as ITestExecutionContext;
			if(testContext != null)
				next = wrap(next, testContext);;
			wrap(next, context)(test, result);
        }

		public void Initialize() {
			fixture.Initialize();
		}
		
		public void Relase() {
			fixture.Release();
		}

        Func<TestContextStep, ITestExecutionContext, TestContextStep> CombineEstablish(IFixtureContext context) {
            return (acc, x) => x.Establish(context, acc);
        }

		IEnumerable<ITestExecutionContext> GetTestContexts(IConeAttributeProvider attributes) {
			return attributes.GetCustomAttributes(typeof(ITestExecutionContext))
				.Cast<ITestExecutionContext>();
		}
    }
}
