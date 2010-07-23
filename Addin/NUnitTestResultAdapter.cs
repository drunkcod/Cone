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
            get {
                switch (result.ResultState) {
                    case ResultState.Ignored: return TestStatus.Pending;
                    case ResultState.Failure: return TestStatus.Failure;
                    default: return TestStatus.Success;
                }
            }
        }
    }
}
