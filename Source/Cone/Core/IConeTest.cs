using System.Reflection;

namespace Cone.Core
{
    public interface IConeTest : IConeEntity
    {
		Assembly Assembly { get; }
        ITestName TestName { get; }
		IConeAttributeProvider Attributes { get; }
		string Location { get; }
		void Run(ITestResult testResult);
    }
}
