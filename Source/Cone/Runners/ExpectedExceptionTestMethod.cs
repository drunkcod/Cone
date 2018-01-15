using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	class ExpectedExceptionTestMethod : ConeTestMethod
	{
		readonly ExpectedTestResult expectedExceptionType;

		public ExpectedExceptionTestMethod(IConeFixture fixture, Invokable method, ExpectedTestResult expectedExceptionType, IEnumerable<string> testCategories) : base(fixture, method, testCategories) {
			this.expectedExceptionType = expectedExceptionType;
		}

		public override void Invoke(object[] parameters, ITestResult result) {
			try {
				Invoke(parameters);
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