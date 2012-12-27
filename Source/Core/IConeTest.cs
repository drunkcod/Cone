using System.Collections.Generic;

namespace Cone.Core
{
    public interface IConeTest : IConeEntity
    {
        ITestName TestName { get; }
		IConeAttributeProvider Attributes { get; }
		void Run(ITestResult testResult);
    }
}
