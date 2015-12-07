namespace Cone.Core
{
	public class DryRunTestExecutor : ITestExecutor
	{
		public void Run(IConeTest test, ITestResult result) {
			result.Success();
		}

		public void Initialize() { }
		public void Relase() { }
	}
}