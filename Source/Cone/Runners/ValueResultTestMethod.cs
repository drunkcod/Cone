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

		public ValueResultTestMethod(IConeFixture fixture, Invokable method, ExpectedTestResult expectedResult, IEnumerable<string> testCategories) : base(fixture, method, testCategories) {
			this.expectedResult = expectedResult;
		}

		public override void Invoke(object[] parameters, ITestResult result) {
			var x = Invoke(parameters);
			if(ReturnType == typeof(void) || expectedResult.Matches(x))
				result.Success();
			else result.TestFailure(new Exception("\n" + ExpectMessages.EqualFormat(x.ToString(), expectedResult.ToString()).ToString()));
		}
	}
}