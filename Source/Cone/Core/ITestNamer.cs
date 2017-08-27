namespace Cone.Core
{
	public interface ITestNamer 
	{
		string NameFor(Invokable method);
		ITestName TestNameFor(string context, Invokable method, object[] parameters);
		string NameFor(Invokable method, object[] parameters);
	}
}
