using System;
using System.Reflection;

namespace Cone.Core
{
    public class ConeFixture : IConeFixture
    {
        readonly Type fixtureType;
        readonly IFixtureHolder fixtureHolder;
        object fixture;

        public ConeFixture(Type fixtureType, IFixtureHolder fixtureHolder) {
            this.fixtureType = fixtureType;
            this.fixtureHolder = fixtureHolder;
        }

        public event EventHandler Before;

        void ITestInterceptor.Before() { 
            InvokeAll(SetupMethods, null);
            Before.Raise(this, EventArgs.Empty);
        }

        void ITestInterceptor.After(ITestResult testResult) {
            InvokeAll(AfterEachWithResult, testResult);
            InvokeAll(TearDownMethods);
        }

        public object Invoke(MethodInfo method, params object[] parameters) {
            return method.Invoke(Fixture, parameters);
        }

        public void Initialize() {
            if(fixture == null)
                fixture = NewFixture();
            InvokeAll(FixtureSetupMethods);
        }

        public void Teardown() {
            InvokeAll(FixtureTeardownMethods);
            fixture = null;
        }

        object NewFixture() { 
            if(FixtureType.IsSealed && FixtureType.GetConstructors().Length == 0)
                return null;
            var ctor = FixtureType.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
                throw new NotSupportedException("No compatible constructor found for " + FixtureType.FullName);
            return ctor.Invoke(null);
        }

        void InvokeAll(MethodInfo[] methods, params object[] parameters) {
            for (int i = 0; i != methods.Length; ++i)
                methods[i].Invoke(Fixture, parameters);
        }

        public Type FixtureType { get { return fixtureType; } }

        public object Fixture { 
            get { return fixture ?? (fixture = NewFixture()); }
        }

        MethodInfo[] FixtureSetupMethods { get { return fixtureHolder.FixtureSetupMethods; } }
        MethodInfo[] FixtureTeardownMethods { get { return fixtureHolder.FixtureTeardownMethods; } }
        MethodInfo[] SetupMethods { get { return fixtureHolder.SetupMethods; } }
        MethodInfo[] TearDownMethods { get { return fixtureHolder.TeardownMethods; } }
        MethodInfo[] AfterEachWithResult { get { return fixtureHolder.AfterEachWithResult; } }

    }
}
