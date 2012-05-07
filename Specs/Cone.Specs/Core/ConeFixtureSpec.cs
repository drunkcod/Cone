using System;
using Cone.Helpers;
using Moq;

namespace Cone.Core
{
    [Describe(typeof(ConeFixture))]
    public class ConeFixtureSpec
    {
        Action<Exception> Nop = _ => { };

        public void disposes_disposable_fixtures() {
            var disposableFixture = new Mock<IDisposable>();
            CreateAndReleaseFixture(disposableFixture.Object.GetType(), _ => disposableFixture.Object);

            disposableFixture.Verify(x => x.Dispose());
        }

        class WithCleanup 
        {
            public ITestCleanup Cleanup;
        }

        public void support_public_test_cleanup() {
            var cleanup = new Mock<ITestCleanup>();
            var fixtureInstance = new WithCleanup{ Cleanup = cleanup.Object };
            CreateAndReleaseFixture(fixtureInstance.GetType(), _ => fixtureInstance);

            cleanup.Verify(x => x.Cleanup());
        }

        static class StaticFixture { }

        public void supports_static_fixtures() {
            var fixture = FixtureFor(typeof(StaticFixture));
            var result = new Mock<ITestResult>().Object;
            fixture.Create(Nop);
            fixture.Release(result.AfterFailure);
        }

        class BrokenFixture
        {
            public void InvalidOperation() {
                throw new InvalidOperationException();
            }
        }
        
        public void report_setup_error_when_failing_to_establish_context() {
            var fixture = FixtureFor(typeof(BrokenFixture));
            (fixture as IConeFixtureMethodSink).BeforeAll(typeof(BrokenFixture).GetMethod("InvalidOperation"));
            
            var error = new ActionSpy<Exception>();
            Verify.That(() => fixture.Create(error) == false);
            Verify.That(() => error.HasBeenCalled);
        }

        public void report_teardown_error_when_failing_release_context() {
            var fixture = FixtureFor(typeof(BrokenFixture));
            (fixture as IConeFixtureMethodSink).AfterAll(typeof(BrokenFixture).GetMethod("InvalidOperation"));
            var error = new ActionSpy<Exception>();
            fixture.Release(error);
            Verify.That(() => error.HasBeenCalled);
        }
	
		ConeFixture FixtureFor(Type type) { return new ConeFixture(type, new string[0]); }
        
        void CreateAndReleaseFixture(Type type, Func<Type, object> fixtureBuilder) {
            var fixture = new ConeFixture(type, new string[0], fixtureBuilder);
            fixture.Create(Nop);
            fixture.Release(Nop);
        }
    }
}
