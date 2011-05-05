using System;
using NUnit.Core;
using System.Reflection;

namespace Cone.Addin
{
    public class NUnitTestResultAdapter : ITestResult
    {
        class NUnitTestNameAdapter : ITestName
        {
            readonly TestName testName;

            public NUnitTestNameAdapter(TestName testName) { this.testName = testName; }
        
            string  ITestName.Name { get { return testName.Name; } }
            string  ITestName.FullName { get { return testName.FullName; } }
        }

        readonly TestResult result;

        public NUnitTestResultAdapter(TestResult result) {
            this.result = result;
        }

        public ITestName TestName {
            get { return new NUnitTestNameAdapter(result.Test.TestName); }
        }

        TestStatus ITestResult.Status {
            get {
                switch (result.ResultState) {
                    case ResultState.Ignored: return TestStatus.Pending;
                    case ResultState.Failure: 
                        switch(result.FailureSite) {
                            case FailureSite.SetUp: return TestStatus.SetupFailure;
                            default: return TestStatus.Failure;
                        }
                    default: return TestStatus.Success;
                }
            }
        }

        void ITestResult.Success() { result.Success(); }
        void ITestResult.Pending(string reason) { result.Ignore(reason); }
        void ITestResult.BeforeFailure(Exception ex) { Failure(ex, FailureSite.SetUp); }
        void ITestResult.TestFailure(Exception ex) { Failure(ex, FailureSite.Test); }
        void ITestResult.AfterFailure(Exception ex) { Failure(ex, FailureSite.TearDown); }

        void Failure(Exception ex, FailureSite site) {
            var invocationException = ex as TargetInvocationException;
            if(invocationException != null)
                ex = invocationException.InnerException;
            result.SetResult(ResultState.Failure, ex.Message, ex.StackTrace, site);
        }

    }
}