using System;
using System.Collections.Generic;
using System.Reflection;
using Cone.Core;
using Cone.Expectations;

namespace Cone.Runners
{
    class ConePadTest : IConeTest
    {
        readonly ITestName name;
        readonly IConeFixture fixture;
        readonly MethodInfo method;
        readonly object[] args;
        readonly IConeAttributeProvider attributes;
    	readonly object expectedResult;

    	public ConePadTest(ITestName name, IConeFixture fixture, MethodInfo method, object[] args, object result, IConeAttributeProvider attributes) {
            this.name = name;
            this.fixture = fixture;
            this.method = method;
            this.args = args;
			this.expectedResult= result;
            this.attributes = attributes;
        }

        public ITestName Name { get { return name; } }

        IConeAttributeProvider IConeTest.Attributes { get { return attributes; } }
		IEnumerable<string> IConeTest.Categories { get { return fixture.Categories; } }
        void IConeTest.Run(ITestResult result) {
			var x = method.Invoke(fixture.Fixture, args);
			if(method.ReturnType == typeof(void) || x.Equals(expectedResult))
				result.Success();
			else result.TestFailure(new Exception("\n" + string.Format(ExpectMessages.EqualFormat, x, expectedResult)));
		}
    }
}
