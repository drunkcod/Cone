using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
	public class ConeFixtureMethodCollection : IConeFixtureMethodSink
	{
		readonly List<Invokable> beforeAll = new List<Invokable>();
		readonly List<Invokable> beforeEach = new List<Invokable>();
		readonly List<Invokable> afterEach = new List<Invokable>();
		readonly List<Invokable> afterAll = new List<Invokable>();

		public void Unintresting(Invokable method) { }
		public void BeforeAll(Invokable method) => beforeAll.Add(method);
		public void BeforeEach(Invokable method) => beforeEach.Add(method);
		public void AfterEach(Invokable method) => afterEach.Add(method);
		public void AfterEachWithResult(Invokable method) => afterEach.Add(method);
		public void AfterAll(Invokable method) => afterAll.Add(method);

		public void InvokeBeforeAll(object target) =>
			InvokeAll(target, beforeAll);

		public void InvokeBeforeEach(object target) =>
			InvokeAll(target, beforeEach);

		public void InvokeAfterEach(object target, ITestResult result) =>
			InvokeAll(target, afterEach, result);

		public void InvokeAfterAll(object target)=>
			InvokeAll(target, afterAll);

		void InvokeAll(object target, List<Invokable> methods, params object[] parameters) {
			var errors = new FixtureException();
			for (var i = 0; i != methods.Count; ++i)
				try {
					var method = methods[i];
					var methodParameters = method.GetParameters();
					method.Await(target, parameters.Length == methodParameters.Length ? parameters : new object[methodParameters.Length]);
				} catch(TargetInvocationException e) {
					errors.Add(e.InnerException);
				}
			if(errors.Count > 0)
				throw errors;
		}
	}
}