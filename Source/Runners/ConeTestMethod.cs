using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Core;

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

		public virtual void Invoke(object[] parameters, ITestResult result) {
			Await(Invoke(parameters));

			result.Success();
		}

		public ParameterInfo[] GetParameters() { return method.GetParameters(); } 

		protected object Invoke(object[] parameters) {
			return fixture.Invoke(method, parameters);
		}

		void Await(object obj) {
			if(obj == null)
				return;
			var wait = obj.GetType().GetMethod("Wait", Type.EmptyTypes);
			if(wait == null)
				return;
			((Action)Delegate.CreateDelegate(typeof(Action), obj, wait))();
		}
	}
}