using Cone.Core;

namespace Cone
{
	public interface ITestContext
	{
		void Before();
		void After(ITestResult result);
	}
}
