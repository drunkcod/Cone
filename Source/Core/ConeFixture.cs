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
		readonly Type fixtureType;
		readonly FixtureProvider fixtureCreator;
		readonly ConeFixtureMethodCollection fixtureMethods = new ConeFixtureMethodCollection();
		readonly IEnumerable<string> categories;
		object fixture;
		bool fixtureInitialized = false;

		public ConeFixture(Type fixtureType, IEnumerable<string> categories, Func<Type, object> fixtureBuilder): 
			this(fixtureType, categories, new LambdaObjectProvider(fixtureBuilder)) { }

		public ConeFixture(Type fixtureType, IEnumerable<string> categories, FixtureProvider fixtureCreator) {
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
				Initialize();
				action(this);
			} catch(Exception ex) {
				beforeFailure(ex);
			} finally {
				try {
					Release();
				} catch(Exception ex) {
					afterFailure(ex);
				}
			}
		}

		public void Initialize() {
			if(fixtureInitialized) 
				return;	
			fixtureMethods.InvokeBeforeAll(EnsureFixture());
			fixtureInitialized = true;
		}

		public void Release() {
			try {
				DoFixtureCleanup();
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
