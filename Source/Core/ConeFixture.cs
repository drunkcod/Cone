using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cone.Core
{
	public class ConeFixture : IConeFixture
    {
        readonly ObjectProvider fixtureCreator;
        readonly Type fixtureType;
        object fixture;
        readonly ConeFixtureMethodCollection fixtureMethods = new ConeFixtureMethodCollection();
		bool fixtureInitialized = false;
		readonly IEnumerable<string> categories; 

	    public ConeFixture(Type fixtureType, IEnumerable<string> categories, Func<Type, object> fixtureBuilder): 
			this(fixtureType, categories, new LambdaObjectProvider(fixtureBuilder)) { }

	    public ConeFixture(Type fixtureType, IEnumerable<string> categories, ObjectProvider fixtureCreator) {
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

		public object GetValue(FieldInfo field) {
			return field.GetValue(Fixture);
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
				Initialize();
                return true;
            } catch(Exception ex) {
                error(ex);
                return false;
            }
        }

        public void Release(Action<Exception> error) {
            try {
				DoFixtureCleanup();
            } catch(Exception ex) {
                error(ex);
            } finally {
				if(fixture != null) {
					fixtureCreator.Release(fixture);
					fixture = null;
				}
			}
        }

	    private void DoFixtureCleanup() {
		    if (!fixtureInitialized) 
				return;
		    fixtureInitialized = false;
		    fixtureMethods.InvokeAfterAll(fixture);
	    }

	    public Type FixtureType { get { return fixtureType; } }

        public IConeFixtureMethodSink FixtureMethods { get { return fixtureMethods; } }

        public object Fixture { get { return EnsureFixture(); } }

        object EnsureFixture() {
            return fixture ?? (fixture = fixtureCreator.NewFixture(FixtureType));
        }
    }
}
