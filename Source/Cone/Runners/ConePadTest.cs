using Cone.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Runners
{
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

		public Assembly Assembly => test.Assembly;
		public ITestName TestName => name;

		IConeAttributeProvider IConeTest.Attributes => attributes;
		string IConeEntity.Name => TestName.FullName;
		IEnumerable<string> IConeEntity.Categories => test.Categories;
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

		static bool KeepOriginal(object arg, Type targetType) {
			return arg == null
				|| targetType == typeof(object)
				|| targetType.IsInstanceOfType(arg)
				|| (targetType.IsEnum && arg.GetType() == typeof(int));
		}
    }
}
