using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Core;
using System.Linq;

namespace Cone.Runners
{
	public class ConeTestMethod 
	{
		readonly IConeFixture fixture;
		readonly Invokable method;

		public ConeTestMethod(IConeFixture fixture, Invokable method) {
			this.fixture = fixture;
			this.method = method;
		}

		public bool IsAsync { get { return method.GetCustomAttributes(true).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute"); } }
		public Assembly Assembly { get { return fixture.FixtureType.Assembly; } }

		public IEnumerable<string> Categories { get { return fixture.Categories; } }
		public Type ReturnType { get { return method.ReturnType; } }
		public string Location => $"{method.ReturnType} {fixture.FixtureType}.{method.Name}({method.GetParameters().Select(x => x.ToString()).Join(", ")})";
		public IConeFixture Fixture => fixture;

		public virtual void Invoke(object[] parameters, ITestResult result) {
			Invoke(parameters);
			result.Success();
		}

		public ParameterInfo[] GetParameters() { return method.GetParameters(); } 

		protected object Invoke(object[] parameters) =>
			method.Await(fixture.GetFixtureInstance(), parameters);
	}
}