using Cone.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Runners
{
	class ConePadTest : IConeTest
	{
		readonly ConePadSuite suite;
		readonly ITestName name;
		readonly object[] args;
		readonly IConeAttributeProvider attributes;
		readonly ConeTestMethod test;

		public ConePadTest(ConePadSuite suite, ITestName name, ConeTestMethod test, object[] args, IConeAttributeProvider attributes) {
			this.suite = suite;
			this.name = name;
			this.args = args;
			this.attributes = attributes;
			this.test = test;
		}

		public Assembly Assembly => test.Assembly;
		public ITestName TestName => name;
		public string Location => test.Location;
		public IConeSuite Suite => suite;

		IConeAttributeProvider IConeTest.Attributes => attributes;
		string IConeEntity.Name => TestName.FullName;
		IEnumerable<string> IConeEntity.Categories => test.Categories;
		void IConeTest.Run(ITestResult result) {
			if(test.IsAsync && test.ReturnType == typeof(void))
				throw new NotSupportedException("async void methods aren't supported");
			test.Invoke(ConvertArgs(args, test.GetParameters()), result);
		}

		static object[] ConvertArgs(object[] args, ParameterInfo[] parameters) {
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

		static object ChangeType(object value, Type conversionType) {
			return KeepOriginal(value, conversionType) 
				? value 
				: Convert.ChangeType(value, conversionType);
		}

		static bool KeepOriginal(object arg, Type targetType) {
			return arg == null
				|| targetType == typeof(object)
				|| targetType.IsInstanceOfType(arg)
				|| (targetType.IsEnum && arg.GetType() == typeof(int));
		}
    }
}
