using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;

namespace Cone.Runners
{
	class ConeTestMethod 
	{
		readonly IConeFixture fixture;
		readonly MethodInfo method;

		public ConeTestMethod(IConeFixture fixture, MethodInfo method) {
			this.fixture = fixture;
			this.method = method;
		}

		public IEnumerable<string> Categories { get { return fixture.Categories; } }
 		public Type ReturnType { get { return method.ReturnType; } }

		public virtual void Invoke(object[] parameters, ITestResult result) {
			method.Invoke(fixture.Fixture, parameters);
			result.Success();
		}

		public ParameterInfo[] GetParameters() { return method.GetParameters(); } 

		protected object Invoke(object[] parameters) {
			return method.Invoke(fixture.Fixture, parameters);
		}
	}

	class ValueResultTestMethod : ConeTestMethod
	{
		readonly object expectedResult;

		public ValueResultTestMethod(IConeFixture fixture, MethodInfo method, object expectedResult) : base(fixture, method) {
			this.expectedResult = expectedResult;
		}

		public override void Invoke(object[] parameters, ITestResult result) {
			var x = Invoke(parameters);
			if(Equals(expectedResult, x))
				result.Success();
			else result.TestFailure(new Exception("\n" + string.Format(ExpectMessages.EqualFormat, x, expectedResult)));
		}

		bool Equals(object expected, object actual) {
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

        public ITestName Name { get { return name; } }

        IConeAttributeProvider IConeTest.Attributes { get { return attributes; } }
		IEnumerable<string> IConeTest.Categories { get { return test.Categories; } }
        void IConeTest.Run(ITestResult result) {
			test.Invoke(ConvertArgs(test.GetParameters()), result);
		}

    	private object[] ConvertArgs(ParameterInfo[] parameters) {
			if(args == null)
				return null;
			var x = new object[args.Length];
			for(var i = 0; i != x.Length; ++i) {
				var parameterType = parameters[i].ParameterType;
				var arg = args[i];
				x[i] = KeepOriginalArg(arg, parameterType) ? arg : Convert.ChangeType(arg, parameterType);
			}
			return x;
    	}

		bool KeepOriginalArg(object arg, Type parameterType) {
			return arg == null
				|| parameterType == typeof(object)
				|| parameterType.IsInstanceOfType(arg);
		}
    }
}
