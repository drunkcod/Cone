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
		bool fixtureInitialized = false;

		IEnumerable<string> categories; 

        public ConeFixture(Type fixtureType, IEnumerable<string> categories): this(fixtureType, categories, NewFixture) 
        { }

        public ConeFixture(Type fixtureType, IEnumerable<string> categories, Func<Type,object> fixtureBuilder) {
            this.fixtureType = fixtureType;
			this.categories = categories;
            this.fixtureBuilder = fixtureBuilder;
        }

        public event EventHandler Before;

		public IEnumerable<string> Categories { get { return categories; } } 

		public void Initialize() {
			if(fixtureInitialized) 
				return;
			
			InvokeAll(beforeAll);
			fixtureInitialized = true;
		}

        void ITestInterceptor.Before() {
			Initialize();
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

        public void WithInitialized(Action<IConeFixture> action, Action<Exception> beforeFailure, Action<Exception> afterFailure) {            
			try {
                if(Create(beforeFailure))
                    action(this);
            } finally {
                Release(afterFailure);
            }
        }

        public bool Create(Action<Exception> error) {
            try {
                EnsureFixture();
                return true;
            } catch(Exception ex) {
                error(ex);
                return false;
            }
        }

        public void Release(Action<Exception> error) {
            try {				
				if(fixtureInitialized) {
					fixtureInitialized = false;
					InvokeAll(afterAll);
				}
                DoCleanup();
                DoDispose();
            } catch(Exception ex) {
                error(ex);
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
