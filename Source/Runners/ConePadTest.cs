using Cone.Core;
using Cone.Expectations;
using System;
using System.Collections.Generic;
using System.Reflection;

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

    class ConePadTest : IConeTest
    {
        readonly ITestName name;
        readonly object[] args;
        readonly IConeAttributeProvider attributes;
		readonly ConeTestMethod test;

    	public ConePadTest(ITestName name, ConeTestMethod test, object[] args, IConeAttributeProvider attributes) {
            this.name = name;
            this.args = args;
            this.attributes = attributes;
			this.test = test;
        }

		public Assembly Assembly { get { return test.Assembly; } }

        public ITestName TestName { get { return name; } }

        IConeAttributeProvider IConeTest.Attributes { get { return attributes; } }
		string IConeEntity.Name { get { return TestName.FullName; } }
		IEnumerable<string> IConeEntity.Categories { get { return test.Categories; } }
        void IConeTest.Run(ITestResult result) {
			if(test.IsAsync && test.ReturnType == typeof(void))
				throw new NotSupportedException("async void methods aren't supported");
			test.Invoke(ConvertArgs(test.GetParameters()), result);
		}

    	private object[] ConvertArgs(ParameterInfo[] parameters) {
			if(args == null)
				return null;
			var x = new object[args.Length];
			for(var i = 0; i != x.Length; ++i) {
				var parameterType = parameters[i].ParameterType;
				var arg = args[i];
				x[i] = ChangeType(arg, parameterType);
			}
			return x;
    	}

		object ChangeType(object value, Type conversionType) {
			return KeepOriginal(value, conversionType) 
				? value 
				: Convert.ChangeType(value, conversionType);
		}

		bool KeepOriginal(object arg, Type targetType) {
			return arg == null
				|| targetType == typeof(object)
				|| targetType.IsInstanceOfType(arg)
				|| (targetType.IsEnum && arg.GetType() == typeof(int));
		}
    }
}
