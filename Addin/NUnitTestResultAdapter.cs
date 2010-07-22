using NUnit.Core;
namespace Cone.Addin
{
    class NUnitTestResultAdapter : ITestResult
    {
        readonly TestResult result;

        public NUnitTestResultAdapter(TestResult result) {
            this.result = result;
        }

        string ITestResult.TestName {
            get { return result.Test.TestName.Name; }
        }

        TestStatus ITestResult.Status {
            get { return result.ResultState == ResultState.Success ? TestStatus.Success : TestStatus.Failure; }
        }
    }
}
