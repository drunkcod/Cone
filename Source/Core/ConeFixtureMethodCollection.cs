using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Cone.Core
{
	[Serializable]
	public class FixtureException : Exception
	{
		readonly List<Exception> innerExceptions = new List<Exception>();
 
		public FixtureException(SerializationInfo info, StreamingContext context): base(info, context) { }

		internal FixtureException() { }


		public int Count { get { return innerExceptions.Count; } }
		public Exception this[int index]{ get { return innerExceptions[index]; } }

		internal void Add(Exception ex) { innerExceptions.Add(ex); }
	}

	public class ConeFixtureMethodCollection : IConeFixtureMethodSink
	{
		readonly List<MethodInfo> beforeAll = new List<MethodInfo>();
		readonly List<MethodInfo> beforeEach = new List<MethodInfo>();
		readonly List<MethodInfo> afterEach = new List<MethodInfo>();
		readonly List<MethodInfo> afterAll = new List<MethodInfo>();

		public void Unintresting(MethodInfo method) { }
		public void BeforeAll(MethodInfo method) { beforeAll.Add(method); }
		public void BeforeEach(MethodInfo method) { beforeEach.Add(method); }
		public void AfterEach(MethodInfo method) { afterEach.Add(method); }
		public void AfterEachWithResult(MethodInfo method) { afterEach.Add(method); }
		public void AfterAll(MethodInfo method) { afterAll.Add(method); }

		public void InvokeBeforeAll(object target) {
			InvokeAll(target, beforeAll);
		}

		public void InvokeBeforeEach(object target) {
			InvokeAll(target, beforeEach);
		}

		public void InvokeAfterEach(object target, ITestResult result) {
			InvokeAll(target, afterEach, result);
		}

		public void InvokeAfterAll(object target) {
			InvokeAll(target, afterAll);
		}

		void InvokeAll(object target, List<MethodInfo> methods, params object[] parameters) {
			var errars = new FixtureException();
			for (var i = 0; i != methods.Count; ++i)
				try {
					var method = methods[i];
					var methodParameters = method.GetParameters();
					method.Invoke(target, parameters.Length == methodParameters.Length ? parameters : new object[methodParameters.Length]);
				} catch(TargetInvocationException e) {
					errars.Add(e.InnerException);
				}
			if(errars.Count > 0)
				throw errars;
		}
	}
}