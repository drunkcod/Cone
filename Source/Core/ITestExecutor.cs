namespace Cone.Core
{
	public interface ITestExecutor
	{
		void Run(IConeTest test, ITestResult result);
		void Initialize();
		void Relase();
	}
}