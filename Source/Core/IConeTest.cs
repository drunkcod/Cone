using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface IConeTest : IConeEntity
    {
		Assembly Assembly { get; }
        ITestName TestName { get; }
		IConeAttributeProvider Attributes { get; }
		void Run(ITestResult testResult);
    }
}
