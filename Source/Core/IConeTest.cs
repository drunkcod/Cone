using System.Collections.Generic;

namespace Cone.Core
{
    public interface IConeTest
    {
        ITestName Name { get; }
		IConeAttributeProvider Attributes { get; }
		IEnumerable<string> Categories { get; }
		void Run(ITestResult testResult);
    }
}
