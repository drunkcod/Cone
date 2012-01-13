using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public class ConeFixture : IConeFixture, IConeFixtureMethodSink
    {
        readonly Func<Type, object> fixtureBuilder;
        readonly Type fixtureType;
        readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
        readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEach = new List<MethodInfo>();
        readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
        readonly List<MethodInfo> afterAll = new List<MethodInfo>();
        object fixture;

        public ConeFixture(Type fixtureType): this(fixtureType, NewFixture) 
        { }

        public ConeFixture(Type fixtureType, Func<Type,object> fixtureBuilder) {
            this.fixtureType = fixtureType;
            this.fixtureBuilder = fixtureBuilder;
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

        public void WithInitialized(ITestResult result, Action action, Action<Exception> error) {
            try {
                if(Create(result, error))
                    action();
            } finally {
                Release(result);
            }
        }

        public bool Create(ITestResult result, Action<Exception> error) {
            try {
                EnsureFixture();
                InvokeAll(beforeAll);
                return true;
            } catch(Exception ex) {
                result.BeforeFailure(ex);
                error(ex);
                return false;
            }
        }

        public void Release(ITestResult result) {
            try {
                InvokeAll(afterAll);
                DoCleanup();
                DoDispose();
            } catch(Exception ex) {
                result.AfterFailure(ex);
            } finally { fixture = null; }
        }

        private void DoDispose() {
            var asDisposable = fixture as IDisposable;
            if (asDisposable != null)
                asDisposable.Dispose();
        }

        private void DoCleanup() {
            var fields = fixtureType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in fields)
                if (typeof(ITestCleanup).IsAssignableFrom(item.FieldType)) {
                    var cleaner = item.GetValue(fixture) as ITestCleanup;
                    cleaner.Cleanup();
                }
        }

        static object NewFixture(Type fixtureType) { 
            if(fixtureType.IsSealed && fixtureType.GetConstructors().Length == 0)
                return null;
            var ctor = fixtureType.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
                throw new NotSupportedException("No compatible constructor found for " + fixtureType.FullName);
            return ctor.Invoke(null);
        }

        void InvokeAll(List<MethodInfo> methods, params object[] parameters) {
            for (int i = 0; i != methods.Count; ++i)
                Invoke(methods[i], parameters);
        }

        public Type FixtureType { get { return fixtureType; } }

        public object Fixture { get { return EnsureFixture(); } }

        object EnsureFixture() {
            return fixture ?? (fixture = fixtureBuilder(FixtureType));
        }

        void IConeFixtureMethodSink.Unintresting(MethodInfo method) { }
        void IConeFixtureMethodSink.BeforeAll(MethodInfo method) { beforeAll.Add(method); }
        void IConeFixtureMethodSink.BeforeEach(MethodInfo method) { beforeEach.Add(method); }
        void IConeFixtureMethodSink.AfterEach(MethodInfo method) { afterEach.Add(method); }
        void IConeFixtureMethodSink.AfterEachWithResult(MethodInfo method) { afterEachWithResult.Add(method); }
        void IConeFixtureMethodSink.AfterAll(MethodInfo method) { afterAll.Add(method); }
    }
}
