using System;
using System.Reflection;

namespace Cone.Core
{

	public class BasicTestNamer : ITestNamer
	{
		readonly ParameterFormatter formatter = new ParameterFormatter();

		public string NameFor(MethodBase method) => $"{method.DeclaringType.FullName}.{method.Name}";

		public string NameFor(MethodInfo method, object[] parameters) =>
			NameFor(method) + "(" + string.Join(", ", Array.ConvertAll(parameters, formatter.Format)) + ")";

		public ITestName TestNameFor(string context, MethodInfo method, object[] parameters) =>
			new ConeTestName(method.DeclaringType.FullName, method.Name.Replace('_', ' '));
	}
}
