using Cone.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Cone.Runners
{
	class ConeTest : IConeTest
	{
		readonly ConeSuite suite;
		readonly ITestName name;
		readonly ConeTestMethod test;
		readonly ConeTestMethodContext context;

		public ConeTest(ConeSuite suite, ITestName name, ConeTestMethod test, ConeTestMethodContext context) {
			this.suite = suite;
			this.name = name;
			this.test = test;
			this.context = context;
		}

		public Assembly Assembly => suite.Fixture.FixtureType.Assembly;
		public ITestName TestName => name;
		public string Location => test.Location;
		public IConeSuite Suite => suite;

		IConeAttributeProvider IConeTest.Attributes => context;
		string IConeEntity.Name => TestName.FullName;
		IEnumerable<string> IConeEntity.Categories => suite.Fixture.Categories.Concat(context.Categories);
		
		void IConeTest.Run(ITestResult result) {
			if(test.IsAsync && test.ReturnType == typeof(void))
				throw new NotSupportedException("async void methods aren't supported");
			test.Invoke(suite.Fixture.GetFixtureInstance(), context.Arguments, result);
		}
    }
}
