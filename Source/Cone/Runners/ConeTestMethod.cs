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
		readonly IEnumerable<string> testCategories;

		public ConeTestMethod(IConeFixture fixture, Invokable method, IEnumerable<string> testCategories) {
			this.fixture = fixture;
			this.method = method;
			this.testCategories = testCategories;
		}

		public bool IsAsync => method.GetCustomAttributes(true).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
		public Assembly Assembly => fixture.FixtureType.Assembly;

		public IEnumerable<string> Categories => fixture.Categories.Concat(testCategories);

		public Type ReturnType => method.ReturnType;
		public string Location => $"{method.ReturnType} {fixture.FixtureType}.{method.Name}({method.GetParameters().Select(x => x.ToString()).Join(", ")})";
		public IConeFixture Fixture => fixture;

		public virtual void Invoke(object[] parameters, ITestResult result) {
			Invoke(parameters);
			result.Success();
		}

		public ParameterInfo[] GetParameters() => method.GetParameters();

		protected object Invoke(object[] parameters) =>
			method.Await(fixture.GetFixtureInstance(), parameters);
	}
}