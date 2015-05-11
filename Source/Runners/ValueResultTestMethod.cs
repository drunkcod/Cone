using System;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;

namespace Cone.Runners
{
	class ValueResultTestMethod : ConeTestMethod
	{
		readonly object expectedResult;

		public ValueResultTestMethod(IConeFixture fixture, MethodInfo method, object expectedResult) : base(fixture, method) {
			this.expectedResult = expectedResult;
		}

		public override void Invoke(object[] parameters, ITestResult result) {
			var x = Invoke(parameters);
			if(ReturnType == typeof(void) || ResultEquals(expectedResult, x))
				result.Success();
			else result.TestFailure(new Exception("\n" + string.Format(ExpectMessages.EqualFormat, x, expectedResult)));
		}

		bool ResultEquals(object expected, object actual) {
			return Convert.ChangeType(actual, expected.GetType()).Equals(expected);
		}
	}
}