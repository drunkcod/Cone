using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Cone.Core;
using System.IO;

namespace Cone.Runners
{
    public class TestSession
    {
        class TestResult : ITestResult
        {
            readonly IConeTest test;

            public TestResult(IConeTest test) {
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

        class TestSessionReport : IConeLogger
        {
            int Passed;
            int Failed { get { return failures.Count; } }
            int Excluded;
            int Total { get { return Passed + Failed + Excluded; } }
            Stopwatch timeTaken;
            readonly List<ConeTestFailure> failures = new List<ConeTestFailure>();

            public void BeginSession() {
                timeTaken = Stopwatch.StartNew();
            }

            public void EndSession() {
                timeTaken.Stop();
            }

            public void WriteInfo(Action<System.IO.TextWriter> output) { }

            public void Success(IConeTest test) { ++Passed; }

            public void Failure(ConeTestFailure failure) { failures.Add(failure); }

            public void Pending(IConeTest test) { }

            public void Skipped(IConeTest test) { ++Excluded; }

            public void WriteReport(TextWriter output) {
                output.WriteLine();
                output.WriteLine("{0} tests found. {1} Passed. {2} Failed. ({3} Skipped)", Total, Passed, Failed, Excluded);

                if (failures.Count > 0) {
                    output.WriteLine("Failures:");
                    failures.ForEach((n, failure) => output.WriteLine("{0}) {1}", n, failure));
                }
                output.WriteLine();
                output.WriteLine("Done in {0}.", timeTaken.Elapsed);
            }

        }

        readonly IConeLogger log;
        readonly TestSessionReport report = new TestSessionReport();

        public TestSession(IConeLogger log) {
            this.log = log;
        }

		public Predicate<IConeTest> ShouldSkipTest = _ => false; 
		public Predicate<IConeSuite> IncludeSuite = _ => true;
		public Func<IConeFixture, Action<IConeTest, ITestResult>> GetResultCollector = x => new TestExecutor(x).Run; 

        public void RunSession(Action<Action<IEnumerable<IConeTest>, IConeFixture>> @do) {
            log.BeginSession();
            report.BeginSession();

            @do(CollectResults);

            log.EndSession();
            report.EndSession();
        }

        void CollectResults(IEnumerable<IConeTest> tests, IConeFixture fixture) {
			var collectResult = GetResultCollector(fixture);
			tests.ForEach(test => {
				if(ShouldSkipTest(test))
                    Skipped(test);
                else 
                    CollectResult(collectResult, test);
			});
        }

        void CollectResult(Action<IConeTest, ITestResult> collectResult, IConeTest test) {
            var result = new TestResult(test);
            collectResult(test, result);
            switch (result.Status) {
                case TestStatus.Success:
                    Success(test);
                    break;
                case TestStatus.SetupFailure: goto case TestStatus.Failure;
                case TestStatus.TeardownFailure: goto case TestStatus.Failure;
                case TestStatus.Failure:
                    Failure(new ConeTestFailure(test.Name, result.Error));
                    break;
                case TestStatus.Pending:
                    Pending(test);
                    break;
            }
        }

        void Success(IConeTest test) {
            log.Success(test);
            report.Success(test);
        }

        void Failure(ConeTestFailure failure) {
            log.Failure(failure);
            report.Failure(failure);
        }

        void Pending(IConeTest test) {
            log.Pending(test);
            report.Pending(test);
        }

        void Skipped(IConeTest test) {
            log.Skipped(test);
            report.Skipped(test);
        }
        
        public void Report() {
            log.WriteInfo(output => report.WriteReport(output));
        }
    }
}
