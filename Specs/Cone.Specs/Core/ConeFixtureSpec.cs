using System;
using System.Collections.Generic;
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

		class SimpleFixture
		{
			public readonly List<string> Executed = new List<string>();
			public string ExecutionPath { get { return string.Join("->", Executed); } }

			public void BeforeAll() { Executed.Add("BeforeAll"); }
			public void AfterAll() { Executed.Add("AfterAll"); }
			public void BeforeEach() { Executed.Add("BeforeEach"); }
		}

		public void create_does_not_execute_BeforeAll() {
            var fixtureInstance = new SimpleFixture();
            var fixture = new ConeFixture(fixtureInstance.GetType(), new string[0], _ => fixtureInstance);
			IConeFixtureMethodSink fixtureMethods = fixture;
			fixtureMethods.BeforeAll( ((Action)fixtureInstance.BeforeAll).Method);
            fixture.Create(Nop);

			Verify.That(() => fixtureInstance.Executed.Count == 0);
		}

		public void BeforeAll_executed_exactly_once() {
            var fixtureInstance = new SimpleFixture();
            var fixture = new ConeFixture(fixtureInstance.GetType(), new string[0], _ => fixtureInstance);
			IConeFixtureMethodSink fixtureMethods = fixture;
			fixtureMethods.BeforeAll( ((Action)fixtureInstance.BeforeAll).Method);
			fixtureMethods.BeforeEach( ((Action)fixtureInstance.BeforeEach).Method);
            fixture.Create(Nop);
			(fixture as ITestInterceptor).Before();
			(fixture as ITestInterceptor).Before();

			Verify.That(() => fixtureInstance.ExecutionPath == "BeforeAll->BeforeEach->BeforeEach");
		}

		public void AfterAll_executeted_for_initialized_fixture_when_released() {
            var fixtureInstance = new SimpleFixture();
            var fixture = new ConeFixture(fixtureInstance.GetType(), new string[0], _ => fixtureInstance);
			IConeFixtureMethodSink fixtureMethods = fixture;
			fixtureMethods.AfterAll(((Action)fixtureInstance.AfterAll).Method);
            fixture.WithInitialized(x => x.Before(), Nop, Nop);

			Verify.That(() => fixtureInstance.ExecutionPath == "AfterAll");
		}


		public void only_runs_AfterAll_if_fixture_has_been_initialized() {
            var fixtureInstance = new SimpleFixture();
            var fixture = new ConeFixture(fixtureInstance.GetType(), new string[0], _ => fixtureInstance);
			IConeFixtureMethodSink fixtureMethods = fixture;
			fixtureMethods.AfterAll(((Action)fixtureInstance.AfterAll).Method);
            fixture.Create(Nop);
			fixture.Release(_ => { });

			Verify.That(() => fixtureInstance.Executed.Count == 0);
		}

		ConeFixture FixtureFor(Type type) { return new ConeFixture(type, new string[0]); }
        
        void CreateAndReleaseFixture(Type type, Func<Type, object> fixtureBuilder) {
            var fixture = new ConeFixture(type, new string[0], fixtureBuilder);
            fixture.Create(Nop);
            fixture.Release(Nop);
        }
    }
}
