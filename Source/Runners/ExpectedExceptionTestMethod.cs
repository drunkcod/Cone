using System;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	class ExpectedExceptionTestMethod : ConeTestMethod
	{
		readonly Type expectedExceptionType;

		public ExpectedExceptionTestMethod(IConeFixture fixture, MethodInfo method, Type expectedExceptionType) : base(fixture, method) {
			this.expectedExceptionType = expectedExceptionType;
		}

		public override void Invoke(object[] parameters, ITestResult result) {
			try {
				Invoke(parameters);
				result.TestFailure(new Exception("Expected exception of type " + expectedExceptionType.FullName));
			} catch(TargetInvocationException te) {
				var e = te.InnerException;
				if(e.GetType() != expectedExceptionType)
					result.TestFailure(new Exception("Expected exception of type " + expectedExceptionType.FullName + " but was " + e.GetType()));
				else result.Success();
			}
		}
	}
}