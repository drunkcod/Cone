using System.Reflection;

namespace Cone.Core
{
    public interface IConeTest
    {
        ITestName Name { get; }
		IConeAttributeProvider Attributes { get; }
		void Run(ITestResult testResult);
    }
}
