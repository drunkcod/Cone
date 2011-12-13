using System.Reflection;

namespace Cone.Core
{
    public interface IConeTest
    {
		IConeAttributeProvider Attributes { get; }
		void Run(ITestResult testResult);
    }
}
