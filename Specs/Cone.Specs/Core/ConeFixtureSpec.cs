using System;
using System.Collections.Generic;
using Moq;

namespace Cone.Core
{
    [Describe(typeof(ConeFixture))]
    public class ConeFixtureSpec
    {
        static Action<Exception> Nop = _ => { };

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
            fixture.Initialize(Nop);
            fixture.Release(result.AfterFailure);
        }

        class BrokenFixture
        {
            public void InvalidOperation() {
                throw new InvalidOperationException();
            }
        }

        [Context("given a SimpleFixture")]
        public class ConeFixtureSimpleFixtureSpec
        {
		    class SimpleFixture
		    {
			    public readonly List<string> Executed = new List<string>();
			    public string ExecutionPath { get { return Executed.Join("->"); } }

			    public void BeforeAll() { Executed.Add("BeforeAll"); }
			    public void AfterAll() { Executed.Add("AfterAll"); }
			    public void BeforeEach() { Executed.Add("BeforeEach"); }
		    }

            SimpleFixture FixtureInstance;
            ConeFixture Fixture;
            IConeFixtureMethodSink FixtureMethods { get { return Fixture.FixtureMethods; } }

            [BeforeEach]
            public void given_SimpleFixture_instance() {
                FixtureInstance = new SimpleFixture();
                Fixture = new ConeFixture(FixtureInstance.GetType(), new string[0], _ => FixtureInstance);
            }

		    public void BeforeAll_executed_exactly_once() {
			    FixtureMethods.BeforeAll( ((Action)FixtureInstance.BeforeAll).Method);
			    FixtureMethods.BeforeEach( ((Action)FixtureInstance.BeforeEach).Method);
                Fixture.Initialize(Nop);
			    (Fixture as ITestContext).Before();
			    (Fixture as ITestContext).Before();

			    Check.That(() => FixtureInstance.ExecutionPath == "BeforeAll->BeforeEach->BeforeEach");
		    }

		    public void AfterAll_executeted_for_initialized_fixture_when_released() {
			    FixtureMethods.AfterAll(((Action)FixtureInstance.AfterAll).Method);
                Fixture.WithInitialized(x => x.Before(), Nop, Nop);

			    Check.That(() => FixtureInstance.ExecutionPath == "AfterAll");
		    }
		}

		ConeFixture FixtureFor(Type type) { return new ConeFixture(type, new string[0], new DefaultObjectProvider()); }
        
        void CreateAndReleaseFixture(Type type, Func<Type, object> fixtureBuilder) {
            var fixture = new ConeFixture(type, new string[0], fixtureBuilder);
            fixture.Initialize(Nop);
            fixture.Release(Nop);
        }
    }
}
