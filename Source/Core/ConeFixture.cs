using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cone.Core
{
	public class FixtureEventArgs : EventArgs
	{
		public readonly object Fixture; 

		public FixtureEventArgs(object fixture) {
			this.Fixture = fixture;
		}
	}

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

		public event EventHandler<FixtureEventArgs> FixtureCreated;

		public IEnumerable<string> Categories { get { return categories; } } 

        [SuppressMessage("Microsoft.Design", "CA1033", Justification = "Should never be called directly")]
        void ITestContext.Before() {
            fixtureMethods.InvokeBeforeEach(fixture);
        }

        [SuppressMessage("Microsoft.Design", "CA1033", Justification = "Should never be called directly")]
        void ITestContext.After(ITestResult testResult) {           
            fixtureMethods.InvokeAfterEach(fixture, testResult);
        }

        public object Invoke(MethodInfo method, params object[] parameters) {
            return method.Invoke(EnsureFixture(), parameters);
        }

		public object GetValue(FieldInfo field) {
			return field.GetValue(EnsureFixture());
		}

        public void WithInitialized(Action<IConeFixture> action, Action<Exception> beforeFailure, Action<Exception> afterFailure) {
			try {
                if(Initialize(beforeFailure))
                    action(this);
            } finally {
                Release(afterFailure);
            }
        }

        public bool Initialize(Action<Exception> error) {
            try {
				DoFixtureSetup();
                return true;
            } catch(Exception ex) {
                error(ex);
                return false;
            }
        }

		void DoFixtureSetup() {
			if(fixtureInitialized) 
				return;	
			fixtureMethods.InvokeBeforeAll(EnsureFixture());
			fixtureInitialized = true;
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
		    fixtureMethods.InvokeAfterAll(fixture);
		    fixtureInitialized = false;
	    }

	    public Type FixtureType { get { return fixtureType; } }

        public IConeFixtureMethodSink FixtureMethods { get { return fixtureMethods; } }

        object EnsureFixture() {
			if(fixture == null) {
				fixture = fixtureCreator.NewFixture(FixtureType);
				FixtureCreated.Raise(this, new FixtureEventArgs(fixture));
			}
			return fixture;
        }
    }
}
