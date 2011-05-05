using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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

        class ConeFixtureContext : ITestContext
        {
            readonly IConeFixture fixture;

            public ConeFixtureContext(IConeFixture fixture) {
                this.fixture = fixture;
            }

            public Action<ITestResult> Establish(Action<ITestResult> next) {
                return result => {
                    Maybe(fixture.Before, () => {
                            Maybe(() => next(result), 
                                result.Success, 
                                result.TestFailure);
                        }, result.BeforeFailure);
                    Maybe(() => fixture.After(result), () => { }, result.AfterFailure);
                };
            }
        }

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

        static void Maybe(Action action, Action then, Action<Exception> fail) {
            try {
                action();
                then();
            } catch (TargetInvocationException ex) {
                fail(ex.InnerException);
            } catch (Exception ex) {
                fail(ex);
            }
        }
    }
}
