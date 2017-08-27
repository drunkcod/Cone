namespace Cone.Core
{
	public interface IConeFixtureMethodSink 
	{
		void Unintresting(Invokable method);
		void BeforeAll(Invokable method);
		void BeforeEach(Invokable  method);
		void AfterEach(Invokable  method);
		void AfterEachWithResult(Invokable method);
		void AfterAll(Invokable method);
	}
}