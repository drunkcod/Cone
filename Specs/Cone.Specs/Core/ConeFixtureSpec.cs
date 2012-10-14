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
            fixture.Create(Nop);
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
			    public string ExecutionPath { get { return string.Join("->", Executed); } }

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

		    public void create_does_not_execute_BeforeAll() {
			    FixtureMethods.BeforeAll( ((Action)FixtureInstance.BeforeAll).Method);
                Fixture.Create(Nop);

			    Verify.That(() => FixtureInstance.Executed.Count == 0);
		    }

		    public void BeforeAll_executed_exactly_once() {
			    FixtureMethods.BeforeAll( ((Action)FixtureInstance.BeforeAll).Method);
			    FixtureMethods.BeforeEach( ((Action)FixtureInstance.BeforeEach).Method);
                Fixture.Create(Nop);
			    (Fixture as ITestInterceptor).Before();
			    (Fixture as ITestInterceptor).Before();

			    Verify.That(() => FixtureInstance.ExecutionPath == "BeforeAll->BeforeEach->BeforeEach");
		    }

		    public void AfterAll_executeted_for_initialized_fixture_when_released() {
			    FixtureMethods.AfterAll(((Action)FixtureInstance.AfterAll).Method);
                Fixture.WithInitialized(x => x.Before(), Nop, Nop);

			    Verify.That(() => FixtureInstance.ExecutionPath == "AfterAll");
		    }

		    public void only_runs_AfterAll_if_fixture_has_been_initialized() {
			    FixtureMethods.AfterAll(((Action)FixtureInstance.AfterAll).Method);
                Fixture.Create(Nop);
			    Fixture.Release(_ => { });

			    Verify.That(() => FixtureInstance.Executed.Count == 0);
		    }
    }

		ConeFixture FixtureFor(Type type) { return new ConeFixture(type, new string[0]); }
        
        void CreateAndReleaseFixture(Type type, Func<Type, object> fixtureBuilder) {
            var fixture = new ConeFixture(type, new string[0], fixtureBuilder);
            fixture.Create(Nop);
            fixture.Release(Nop);
        }
    }
}
