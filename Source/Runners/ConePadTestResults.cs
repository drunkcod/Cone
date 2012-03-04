using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
    class ConePadTestResults
    {
        class ConePadTestResult : ITestResult
        {
            TestStatus testStatus;
            IConeTest test;

            public ConePadTestResult(IConeTest test) {
                this.test = test;
            }

            public Exception Error;

            public ITestName TestName { get { return test.Name; } }

            public TestStatus Status { get { return testStatus; } }

            void ITestResult.Success() { testStatus = TestStatus.Success; }

            void ITestResult.Pending(string reason) { testStatus = TestStatus.Pending; }
            
            void ITestResult.BeforeFailure(Exception ex) { 
                testStatus = TestStatus.SetupFailure;
                Error = ex;
            }

            void ITestResult.TestFailure(Exception ex) {
                testStatus = TestStatus.Failure;
                Error = ex;
            }
            
            void ITestResult.AfterFailure(Exception ex) {
                testStatus = TestStatus.TeardownFailure;
                Error = ex;
            }
        }

        readonly IConeLogger log;
        readonly List<KeyValuePair<ConePadTest, Exception>> failures = new List<KeyValuePair<ConePadTest, Exception>>();
        int passed;
        Stopwatch timeTaken;

        public ConePadTestResults(IConeLogger log) {
            this.log = log;
        }

        public bool ShowProgress { get; set; }

        int Passed { get { return passed; } }
        int Failed { get { return failures.Count; } }
        int Total { get { return Passed + Failed; } }

        public void BeginTestSession() { timeTaken = Stopwatch.StartNew(); }
        public void EndTestSession() { timeTaken.Stop(); }

        public void BeginTest(ConePadTest test, Action<ITestResult> collectResult) {
            var result = new ConePadTestResult(test);
            collectResult(result);
            switch(result.Status) {
                case TestStatus.Success: 
                    ++passed; 
                    LogProgress(".");
                    break;
                case TestStatus.SetupFailure: goto case TestStatus.Failure;
                case TestStatus.Failure:
                    failures.Add(new KeyValuePair<ConePadTest,Exception>(test, result.Error)); 
                    LogProgress("F");
                    break;
                case TestStatus.Pending:
                    LogProgress("?");
                    break;
            }
        }

        void LogProgress(string message) {
            if(ShowProgress)
                log.Info(message);
        }

        public void Report() {
            LogProgress("\n");
            log.Info("{0} tests ran. {1} Passed. {2} Failed.\n", Total, Passed, Failed);

            if(failures.Count > 0) {
                log.Info("Failures:\n");

                for(var i = 0; i != failures.Count; ++i) {
                    var item = failures[i];
                    var ex = item.Value;
                    var invocationException = ex as TargetInvocationException;
                    if (invocationException != null)
                        ex = invocationException.InnerException;
                    log.Info("  {0,2})", i + 1);

                    log.Failure(new ConeTestFailure(item.Key.Name, ex, 3));
                }
            }
            log.Info("\nDone in {0}.\n", timeTaken.Elapsed);
        }
    }
}
