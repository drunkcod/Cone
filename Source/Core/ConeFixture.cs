using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public class ConeFixture : IConeFixture, IConeFixtureMethodSink
    {
        readonly Type fixtureType;
        readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
        readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
        readonly List<MethodInfo> afterAll = new List<MethodInfo>();

        object fixture;

        public ConeFixture(Type fixtureType) {
            this.fixtureType = fixtureType;
        }

        public event EventHandler Before;

        void ITestInterceptor.Before() { 
            InvokeAll(beforeEach, null);
            Before.Raise(this, EventArgs.Empty);
        }

        void ITestInterceptor.After(ITestResult testResult) {
            InvokeAll(afterEachWithResult, testResult);
            InvokeAll(afterEach);
        }

        public object Invoke(MethodInfo method, params object[] parameters) {
            return method.Invoke(Fixture, parameters);
        }

        public void Initialize() {
            if(fixture == null)
                fixture = NewFixture();
            InvokeAll(beforeAll);
        }

        public void Teardown() {
            InvokeAll(afterAll);
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

        void InvokeAll(List<MethodInfo> methods, params object[] parameters) {
            for (int i = 0; i != methods.Count; ++i)
                Invoke(methods[i], parameters);
        }

        public Type FixtureType { get { return fixtureType; } }

        public object Fixture { 
            get { return fixture ?? (fixture = NewFixture()); }
        }

        void IConeFixtureMethodSink.Unintresting(MethodInfo method) { }
        void IConeFixtureMethodSink.BeforeAll(MethodInfo method) { beforeAll.Add(method); }
        void IConeFixtureMethodSink.BeforeEach(MethodInfo method) { beforeEach.Add(method); }
        void IConeFixtureMethodSink.AfterEach(MethodInfo method) { afterEach.Add(method); }
        void IConeFixtureMethodSink.AfterEachWithResult(MethodInfo method) { afterEachWithResult.Add(method); }
        void IConeFixtureMethodSink.AfterAll(MethodInfo method) { afterAll.Add(method); }
    }
}
