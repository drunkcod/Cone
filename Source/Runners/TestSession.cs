﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
    public class TestSession
    {
        class SessionTestResult : ITestResult
        {
            readonly IConeTest test;

            public SessionTestResult(IConeTest test) {
                this.test = test;
            }

            public Exception Error;

            public ITestName TestName { get { return test.Name; } }

            public TestStatus Status { get; private set; }

            void ITestResult.Success() { Status = TestStatus.Success; }

            void ITestResult.Pending(string reason) { Status = TestStatus.Pending; }
            
            void ITestResult.BeforeFailure(Exception ex) { 
                Status = TestStatus.SetupFailure;
                Error = ex;
            }

            void ITestResult.TestFailure(Exception ex) {
                Status = TestStatus.Failure;
                Error = ex;
            }
            
            void ITestResult.AfterFailure(Exception ex) {
                Status = TestStatus.TeardownFailure;
                Error = ex;
            }
        }

        readonly IConeLogger log;
        readonly List<KeyValuePair<IConeTest, Exception>> failures = new List<KeyValuePair<IConeTest, Exception>>();
        Stopwatch timeTaken;

        public TestSession(IConeLogger log) {
            this.log = log;
        }

        public bool ShowProgress { get; set; }
		public Predicate<IConeTest> ShouldSkipTest = _ => false; 
		public Predicate<IConeFixture> ShouldSkipFixture = _ => false; 

        int Passed;
        int Failed { get { return failures.Count; } }
        int Total { get { return Passed + Failed + Skipped; } }
		int Skipped;

        public void BeginSession() { timeTaken = Stopwatch.StartNew(); }
        public void EndSession() { timeTaken.Stop(); }

        public void CollectResults(IEnumerable<IConeTest> tests, Action<IConeTest, ITestResult> collectResult) {
			tests.ForEach(test => {
				if(ShouldSkipTest(test)) { 
					++Skipped;
					return;
				}

				var result = new SessionTestResult(test);
				collectResult(test, result);
				switch(result.Status) {
					case TestStatus.Success:
						AddSuccess(test);
						break;
					case TestStatus.SetupFailure: goto case TestStatus.Failure;
					case TestStatus.Failure:
						AddFailure(test, result.Error);
						break;
					case TestStatus.Pending:
						AddPending(test);
						break;
				}
			});
        }

        void AddSuccess(IConeTest test) {
            ++Passed;
            log.Success(test);
        }

        void AddFailure(IConeTest test, Exception error) {
            failures.Add(new KeyValuePair<IConeTest, Exception>(test, error));
            LogProgress("F");
        }

        void AddPending(IConeTest test) {
            log.Pending(test);
        }

        void LogProgress(string message) {
            if(ShowProgress)
                log.Info(message);
        }
        
        public void Report() {
            log.Info("\n{0} tests ran. {1} Passed. {2} Failed. ({3} Skipped)\n", Total, Passed, Failed, Skipped);

            if(failures.Count > 0) {
                log.Info("Failures:\n");
                failures.ForEach((i, item) => {
                    var ex = item.Value;
                    var invocationException = ex as TargetInvocationException;
                    if (invocationException != null)
                        ex = invocationException.InnerException;
                    log.Failure(new ConeTestFailure(i + 1, item.Key.Name, ex, 3));
                });
            }
            log.Info("\nDone in {0}.\n", timeTaken.Elapsed);
        }
    }
}