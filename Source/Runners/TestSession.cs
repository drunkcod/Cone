using System;
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
        readonly List<ConeTestFailure> failures = new List<ConeTestFailure>();
        Stopwatch timeTaken;

        public TestSession(IConeLogger log) {
            this.log = log;
        }

		public Predicate<IConeTest> ShouldSkipTest = _ => false; 
		public Predicate<IConeSuite> IncludeSuite = _ => true;
		public Func<IConeFixture, Action<IConeTest, ITestResult>> GetResultCollector = x => new TestExecutor(x).Run; 

        int Passed;
        int Failed { get { return failures.Count; } }
        int Total { get { return Passed + Failed + Skipped; } }
		int Skipped;

        public void BeginSession() { 
			log.BeginSession();
			timeTaken = Stopwatch.StartNew(); 
		}
        public void EndSession() { 
			timeTaken.Stop(); 
			log.EndSession();
		}

        public void CollectResults(IEnumerable<IConeTest> tests, IConeFixture fixture) {
			var collectResult = GetResultCollector(fixture);
			tests.ForEach(test => {
				if(ShouldSkipTest(test)) { 
					++Skipped;
					return;
				}

				var result = new SessionTestResult(test);
				collectResult(test, result);
				switch(result.Status) {
					case TestStatus.Success:
						Success(test);
						break;
					case TestStatus.SetupFailure: goto case TestStatus.Failure;
					case TestStatus.TeardownFailure: goto case TestStatus.Failure;
					case TestStatus.Failure:
						Failure(test, result.Error);
						break;
					case TestStatus.Pending:
						Pending(test);
						break;
				}
			});
        }

        void Success(IConeTest test) {
            ++Passed;
            log.Success(test);
        }

        void Failure(IConeTest test, Exception error) {
            var invocationException = error as TargetInvocationException;
            if (invocationException != null)
                error = invocationException.InnerException;
        	var failure = new ConeTestFailure(failures.Count + 1, test.Name, error);
			failures.Add(failure);
            log.Failure(failure);
        }

        void Pending(IConeTest test) {
            log.Pending(test);
        }
        
        public void Report() {
			log.Info(string.Empty);
            log.Info("{0} tests found. {1} Passed. {2} Failed. ({3} Skipped)", Total, Passed, Failed, Skipped);

            if(failures.Count > 0) {
                log.Info("Failures:");
                failures.ForEach(failure => log.Info("{0}", failure));
            }
			log.Info(string.Empty);
            log.Info("Done in {0}.", timeTaken.Elapsed);
        }
    }
}
