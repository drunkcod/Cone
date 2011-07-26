using System;
using Moq;

namespace Cone.Core
{
    [Describe(typeof(ConeFixture))]
    public class ConeFixtureSpec
    {
        public void disposes_disposable_fixtures() {
            var disposableFixture = new Mock<IDisposable>();
            var fixture = new ConeFixture(disposableFixture.Object.GetType(), _ => disposableFixture.Object);
            fixture.Create();
            fixture.Release();
            disposableFixture.Verify(x => x.Dispose());
        }

        class WithCleanup 
        {
            public ITestCleanup Cleanup;
        }

        public void support_public_test_cleanup() {
            var cleanup = new Mock<ITestCleanup>();
            var fixtureInstance = new WithCleanup{ Cleanup = cleanup.Object };
            var fixture = new ConeFixture(fixtureInstance.GetType(), _ => fixtureInstance);
            fixture.Create();
            fixture.Release();
            cleanup.Verify(x => x.Cleanup());
        }

        static class StaticFixture { }

        public void supports_static_fixtures() {
            var fixture = new ConeFixture(typeof(StaticFixture));
            fixture.Create();
            fixture.Release();
        }
    }
}
