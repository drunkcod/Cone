using System;
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
            var fixture = new ConeFixture(typeof(StaticFixture));
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
            var fixture = new ConeFixture(typeof(BrokenFixture));
            (fixture as IConeFixtureMethodSink).BeforeAll(typeof(BrokenFixture).GetMethod("InvalidOperation"));
            var result = new Mock<ITestResult>();
            Verify.That(() => fixture.Create(result.Object.BeforeFailure) == false);
            result.Verify(x => x.BeforeFailure(It.IsAny<Exception>()));
        }

        public void report_teardown_error_when_failing_release_context() {
            var fixture = new ConeFixture(typeof(BrokenFixture));
            (fixture as IConeFixtureMethodSink).AfterAll(typeof(BrokenFixture).GetMethod("InvalidOperation"));
            var result = new Mock<ITestResult>();
            fixture.Release(result.Object.AfterFailure);
            result.Verify(x => x.AfterFailure(It.IsAny<Exception>()));
        }

        
        void CreateAndReleaseFixture(Type type, Func<Type, object> fixtureBuilder) {
            var fixture = new ConeFixture(type, fixtureBuilder);
            var result = new Mock<ITestResult>().Object;
            fixture.Create(Nop);
            fixture.Release(result.AfterFailure);
        }

    }
}
