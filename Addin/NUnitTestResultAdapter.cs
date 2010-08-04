using System;
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

        void ITestResult.Success() { result.Success(); }
        void ITestResult.Pending(string reason) { result.Ignore(reason); }
        void ITestResult.TestFailure(Exception error) { result.SetResult(ResultState.Failure, error); }
    }
}
