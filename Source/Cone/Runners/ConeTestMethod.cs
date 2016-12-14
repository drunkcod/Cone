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
		readonly MethodInfo method;

		public ConeTestMethod(IConeFixture fixture, MethodInfo method) {
			this.fixture = fixture;
			this.method = method;
		}

		public bool IsAsync { get { return method.GetCustomAttributes(true).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute"); } }
		public Assembly Assembly { get { return method.DeclaringType.Assembly; } }

		public IEnumerable<string> Categories { get { return fixture.Categories; } }
		public Type ReturnType { get { return method.ReturnType; } }
		public string Location => $"{method.ReturnType} {fixture.FixtureType}.{method.Name}({method.GetParameters().Select(x => x.ToString()).Join(", ")})";
		public IConeFixture Fixture => fixture;

		public virtual void Invoke(object[] parameters, ITestResult result) {
			Await(Invoke(parameters));

			result.Success();
		}

		public ParameterInfo[] GetParameters() { return method.GetParameters(); } 

		protected object Invoke(object[] parameters) {
			return fixture.Invoke(method, parameters);
		}

		public static bool IsWaitable(Type type) {
			MethodInfo wait;
			return TryGetWait(type, out wait);
		}

		static bool TryGetWait(Type type, out MethodInfo wait) {
			wait = type.GetMethod("Wait", Type.EmptyTypes);
			return wait != null;
		}

		static void Await(object obj) {
			MethodInfo wait;
			if(obj == null || !TryGetWait(obj.GetType(), out wait))
				return;
			((Action)Delegate.CreateDelegate(typeof(Action), obj, wait))();
		}
	}
}