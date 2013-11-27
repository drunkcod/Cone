using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
	public class ConeFixture : IConeFixture
    {
        readonly FixtureCreator fixtureCreator;
        readonly Type fixtureType;
        object fixture;
        readonly ConeFixtureMethodCollection fixtureMethods = new ConeFixtureMethodCollection();
		bool fixtureInitialized = false;
		readonly IEnumerable<string> categories; 

        public ConeFixture(Type fixtureType, IEnumerable<string> categories): 
			this(fixtureType, categories, new DefaultFixtureCreator()) { }

	    public ConeFixture(Type fixtureType, IEnumerable<string> categories, Func<Type, object> fixtureBuilder): 
			this(fixtureType, categories, new LambdaFixtureCreator(fixtureBuilder)) { }

	    public ConeFixture(Type fixtureType, IEnumerable<string> categories, FixtureCreator fixtureCreator) {
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
				DoFixtureCleanup();
                fixtureCreator.Release(fixture);
            } catch(Exception ex) {
                error(ex);
            } finally { fixture = null; }
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

	public abstract class FixtureCreator
	{
		public abstract object NewFixture(Type fixtureType);
		
		public void Release(object fixture) {
			DoCleanup(fixture);

			var asDisposable = fixture as IDisposable;
            if (asDisposable != null)
                asDisposable.Dispose();
        }

		private void DoCleanup(object fixture) {
			var fixtureType = fixture.GetType();
            var fields = fixtureType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in fields.Where(x => typeof(ITestCleanup).IsAssignableFrom(x.FieldType))) {
				var cleaner = item.GetValue(fixture) as ITestCleanup;
				cleaner.Cleanup();
			}
		}
	}

	class LambdaFixtureCreator : FixtureCreator
	{
		private readonly Func<Type, object> newFixture;

		public LambdaFixtureCreator(Func<Type, object> newFixture) {
			this.newFixture = newFixture;
		}

		public override object NewFixture(Type fixtureType) {
			return newFixture(fixtureType);
		}
	}

	class DefaultFixtureCreator : FixtureCreator
	{
		public override object NewFixture(Type fixtureType) { 
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
