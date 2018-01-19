using System;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	class ExpectedExceptionTestMethod : ConeTestMethod
	{
		readonly ExpectedTestResult expectedExceptionType;

		public ExpectedExceptionTestMethod(Invokable method, ExpectedTestResult expectedExceptionType) : base(method) {
			this.expectedExceptionType = expectedExceptionType;
		}

		public override void Invoke(object fixture, object[] parameters, ITestResult result) {
			try {
				Invoke(fixture, parameters);
				result.TestFailure(new Exception("Expected exception of type " + expectedExceptionType));
			} catch(TargetInvocationException te) {
				var e = te.InnerException;
				if(!expectedExceptionType.Matches(e))
					result.TestFailure(new Exception("Expected exception of type " + expectedExceptionType + " but was " + e.GetType()));
				else result.Success();
			}
		}
	}
}