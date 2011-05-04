using System;
using System.Reflection;

namespace Cone
{
    public interface IFixtureHolder
    {
        Type FixtureType { get; }
        object Fixture { get; set; }
        MethodInfo[] SetupMethods { get; }
        MethodInfo[] TeardownMethods { get; }
        MethodInfo[] AfterEachWithResult { get; }
    }

    public class ConeFixture : IConeFixture
    {
        readonly IFixtureHolder fixtureHolder;
        public ConeFixture(IFixtureHolder fixtureHolder) {
            this.fixtureHolder = fixtureHolder;
        }

        public void Before() { FixtureInvokeAll(SetupMethods, null); }

        public void After(ITestResult testResult) {
            FixtureInvokeAll(AfterEachWithResult, new[] { testResult });
            FixtureInvokeAll(TearDownMethods, null);
        }

        public object Invoke(MethodInfo method, params object[] parameters) {
            return method.Invoke(Fixture, parameters);
        }

        object NewFixture() { 
            var ctor = (this as IConeFixture).FixtureType.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
                return null;
            return ctor.Invoke(null);
        }

        void FixtureInvokeAll(MethodInfo[] methods, object[] parameters) {
            for (int i = 0; i != methods.Length; ++i)
                methods[i].Invoke(Fixture, parameters);
        }

        Type IConeFixture.FixtureType { get { return fixtureHolder.FixtureType; } }

        public object Fixture { 
            get { return fixtureHolder.Fixture ?? (Fixture = NewFixture()); }
            set { fixtureHolder.Fixture = value; }
        }

        MethodInfo[] SetupMethods { get { return fixtureHolder.SetupMethods; } }
        MethodInfo[] TearDownMethods { get { return fixtureHolder.TeardownMethods; } }
        MethodInfo[] AfterEachWithResult { get { return fixtureHolder.AfterEachWithResult; } }

    }
}
