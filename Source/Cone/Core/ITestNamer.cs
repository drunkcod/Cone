using System.Reflection;

namespace Cone.Core
{
	public interface ITestNamer 
	{
		string NameFor(MethodBase method);
		ITestName TestNameFor(string context, MethodInfo method, object[] parameters);
		string NameFor(MethodInfo method, object[] parameters);
	}
}
