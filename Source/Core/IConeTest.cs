using System.Reflection;

namespace Cone.Core
{
    public interface IConeTest
    {
		ICustomAttributeProvider Attributes { get; }
		void Run(ITestResult testResult);
    }
}
