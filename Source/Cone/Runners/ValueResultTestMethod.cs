using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;

namespace Cone.Runners
{
	class ValueResultTestMethod : ConeTestMethod
	{
		readonly ExpectedTestResult expectedResult;

		public ValueResultTestMethod(Invokable method, ExpectedTestResult expectedResult) : base(method) {
			this.expectedResult = expectedResult;
		}

		public override void Invoke(object fixture, object[] parameters, ITestResult result) {
			var x = Invoke(fixture, parameters);
			if(ReturnType == typeof(void) || expectedResult.Matches(x))
				result.Success();
			else result.TestFailure(new Exception("\n" + ExpectMessages.EqualFormat(x.ToString(), expectedResult.ToString()).ToString()));
		}
	}
}