using System.Reflection;

namespace Cone.Core
{
	public interface IConeFixtureMethodSink 
	{
		void Unintresting(MethodInfo method);
		void BeforeAll(MethodInfo method);
		void BeforeEach(MethodInfo method);
		void AfterEach(MethodInfo method);
		void AfterEachWithResult(MethodInfo method);
		void AfterAll(MethodInfo method);
	}
}