using System;
using System.Reflection;
using Cone.Core;
using System.Linq;

namespace Cone.Runners
{
	public class ConeTestMethod 
	{
		readonly Invokable method;

		public ConeTestMethod(Invokable method) {
			this.method = method;
		}

		public bool IsAsync => method.IsAsync;
		public Type ReturnType => method.ReturnType;
		public string Location => method.Location;

		public virtual void Invoke(object fixture, object[] parameters, ITestResult result) {
			Invoke(fixture, parameters);
			result.Success();
		}

		protected object Invoke(object fixture, object[] arguments) =>
			method.Await(fixture, ConvertArgs(arguments, method.GetParameters()));

		static object[] ConvertArgs(object[] args, ParameterInfo[] parameters) {
			if (args == null)
				return null;
			var x = new object[args.Length];
			for (var i = 0; i != x.Length; ++i) {
				var parameterType = parameters[i].ParameterType;
				var arg = args[i];
				x[i] = ChangeType(arg, parameterType);
			}
			return x;
		}

		static object ChangeType(object value, Type conversionType) =>
			KeepOriginal(value, conversionType)
			? value
			: Convert.ChangeType(value, conversionType);

		static bool KeepOriginal(object arg, Type targetType) =>
			arg == null
			|| targetType == typeof(object)
			|| targetType.IsInstanceOfType(arg)
			|| (targetType.IsEnum && arg.GetType() == typeof(int));

	}
}