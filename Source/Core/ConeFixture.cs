using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cone.Core
{
    public class ConeFixture : IConeFixture
    {
        class ConeFixtureMethodCollection : IConeFixtureMethodSink
        {
            readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
            readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
            readonly List<MethodInfo> afterEach = new List<MethodInfo>();
            readonly List<MethodInfo> afterEachWithResult = new List<MethodInfo>();
            readonly List<MethodInfo> afterAll = new List<MethodInfo>();

            public void Unintresting(MethodInfo method) { }
            public void BeforeAll(MethodInfo method) { beforeAll.Add(method); }
            public void BeforeEach(MethodInfo method) { beforeEach.Add(method); }
            public void AfterEach(MethodInfo method) { afterEach.Add(method); }
            public void AfterEachWithResult(MethodInfo method) { afterEachWithResult.Add(method); }
            public void AfterAll(MethodInfo method) { afterAll.Add(method); }

            public void InvokeBeforeAll(object target) {
                InvokeAll(target, beforeAll);
            }

            public void InvokeBeforeEach(object target) {
                InvokeAll(target, beforeEach);
            }

            public void InvokeAfterEach(object target, ITestResult result) {
                InvokeAll(target, afterEachWithResult, result);
                InvokeAll(target, afterEach);
            }

            public void InvokeAfterAll(object target) {
                InvokeAll(target, afterAll);
            }

            void InvokeAll(object target, List<MethodInfo> methods, params object[] parameters) {
                for (int i = 0; i != methods.Count; ++i)
                    methods[i].Invoke(target, parameters);
            }
        }

        readonly IFixtureCreator fixtureCreator;
        readonly Type fixtureType;
        object fixture;
        readonly ConeFixtureMethodCollection fixtureMethods = new ConeFixtureMethodCollection();
		bool fixtureInitialized = false;

		IEnumerable<string> categories; 

        public ConeFixture(Type fixtureType, IEnumerable<string> categories): 
			this(fixtureType, categories, new DefaultFixtureCreator()) { }

	    public ConeFixture(Type fixtureType, IEnumerable<string> categories, Func<Type, object> fixtureBuilder): 
			this(fixtureType, categories, new LambdaFixtureCreator(fixtureBuilder)) { }

	    public ConeFixture(Type fixtureType, IEnumerable<string> categories, IFixtureCreator fixtureCreator) {
            this.fixtureType = fixtureType;
			this.categories = categories;
            this.fixtureCreator = fixtureCreator;
        }

        public event EventHandler Before;

		public IEnumerable<string> Categories { get { return categories; } } 

		public void Initialize() {
			if(fixtureInitialized) 
				return;	
			fixtureMethods.InvokeBeforeAll(Fixture);
			fixtureInitialized = true;
		}

        [SuppressMessage("Microsoft.Design", "CA1033", Justification = "Should never be called directly")]
        void ITestContext.Before() {
			Initialize();
            fixtureMethods.InvokeBeforeEach(Fixture);
            Before.Raise(this, EventArgs.Empty);
        }

        [SuppressMessage("Microsoft.Design", "CA1033", Justification = "Should never be called directly")]
        void ITestContext.After(ITestResult testResult) {           
            fixtureMethods.InvokeAfterEach(Fixture, testResult);
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
					fixtureMethods.InvokeAfterAll(fixture);
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

	    public Type FixtureType { get { return fixtureType; } }

        public IConeFixtureMethodSink FixtureMethods { get { return fixtureMethods; } }

        public object Fixture { get { return EnsureFixture(); } }

        object EnsureFixture() {
            return fixture ?? (fixture = fixtureCreator.NewFixture(FixtureType));
        }
    }

	public interface IFixtureCreator
	{
		object NewFixture(Type fixtureType);
	}

	class LambdaFixtureCreator : IFixtureCreator
	{
		private readonly Func<Type, object> newFixture;

		public LambdaFixtureCreator(Func<Type, object> newFixture) {
			this.newFixture = newFixture;
		}

		public object NewFixture(Type fixtureType) {
			return newFixture(fixtureType);
		}
	}

	class DefaultFixtureCreator : IFixtureCreator
	{
		public object NewFixture(Type fixtureType) { 
            if(IsStatic(fixtureType))
                return null;
            var ctor = fixtureType.GetConstructor(Type.EmptyTypes);
            if(ctor == null)
                throw new NotSupportedException("No compatible constructor found for " + fixtureType.FullName);
            return ctor.Invoke(null);
        }

	    private static bool IsStatic(Type fixtureType) {
		    return fixtureType.IsSealed && fixtureType.GetConstructors().Length == 0;
	    }
	}
}
