using Cone.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        class TestSessionReport : IConeLogger, ISessionLogger
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

            public IConeLogger BeginTest(IConeTest test) {
                return this;
            }

            public void WriteInfo(Action<TextWriter> output) { }

            public void Success() { ++Passed; }

            public void Failure(ConeTestFailure failure) { failures.Add(failure); }

            public void Pending() { }

            public void Skipped() { ++Excluded; }

            public void WriteReport(TextWriter output) {
                output.WriteLine();
                output.WriteLine("{0} tests found. {1} Passed. {2} Failed. ({3} Skipped)", Total, Passed, Failed, Excluded);

                if (failures.Count > 0) {
                    output.WriteLine("Failures:");
                    failures.Each((n, failure) => output.WriteLine("{0}) {1}\n", n, failure));
                }
                output.WriteLine();
                output.WriteLine("Done in {0}.", timeTaken.Elapsed);
            }

        }

        class MulticastSessionLogger : ISessionLogger
        {
            readonly List<ISessionLogger> children = new List<ISessionLogger>();

            public MulticastSessionLogger(params ISessionLogger[] sessionLoggers) {
                this.children.AddRange(sessionLoggers);
            }

            public void Add(ISessionLogger log) {
                children.Add(log);
            }

            public void BeginSession() {
                children.ForEach(x => x.BeginSession());
            }

            public IConeLogger BeginTest(IConeTest test) {
                var log = new MulticastLogger();
                children.ForEach(x => log.Add(x.BeginTest(test)));
                return log;
            }

            public void EndSession() {
                children.ForEach(x => x.EndSession());
            }

            public void WriteInfo(Action<TextWriter> output) {
                using (var outputResult = new StringWriter()) {
                    output(outputResult);
                    var result = outputResult.ToString();
                    children.ForEach(x => x.WriteInfo(writer => writer.Write(result)));
                }
            }
        }

        class MulticastLogger : IConeLogger
        {
            readonly List<IConeLogger> children = new List<IConeLogger>();

            public void Add(IConeLogger log) {
                children.Add(log);
            }

            public void Failure(ConeTestFailure failure) {
                children.ForEach(x => x.Failure(failure));
            }

            public void Success() {
                children.ForEach(x => x.Success());
            }

            public void Pending() {
                children.ForEach(x => x.Pending());
            }

            public void Skipped() {
                children.ForEach(x => x.Skipped());
            }
        }

        readonly ISessionLogger sessionLog;
        readonly TestSessionReport report = new TestSessionReport();

        public TestSession(ISessionLogger sessionLog) {
            this.sessionLog = new MulticastSessionLogger(sessionLog, report);
        }

		public Predicate<IConeTest> ShouldSkipTest = _ => false; 
		public Predicate<IConeSuite> IncludeSuite = _ => true;
		public Func<IConeFixture, Action<IConeTest, ITestResult>> GetResultCollector = x => new TestExecutor(x).Run; 

        public void RunSession(Action<Action<IEnumerable<IConeTest>, IConeFixture>> @do) {
            sessionLog.BeginSession();
            @do(CollectResults);
            sessionLog.EndSession();
        }

        void CollectResults(IEnumerable<IConeTest> tests, IConeFixture fixture) {
			var collectResult = GetResultCollector(fixture);
			tests.Each(test => {
                var log = sessionLog.BeginTest(test);
				if(ShouldSkipTest(test))
                    log.Skipped();
                else
                    CollectResult(collectResult, test, log);
			});
        }

        void CollectResult(Action<IConeTest, ITestResult> collectResult, IConeTest test, IConeLogger log) {
            var result = new TestResult(test);
            collectResult(test, result);
            switch (result.Status) {
                case TestStatus.Success:
                    log.Success();
                    break;
                case TestStatus.SetupFailure: goto case TestStatus.Failure;
                case TestStatus.TeardownFailure: goto case TestStatus.Failure;
                case TestStatus.Failure:
                    log.Failure(new ConeTestFailure(test.Name, result.Error));
                    break;
                case TestStatus.Pending:
                    log.Pending();
                    break;
            }
        }
       
        public void Report() {
            sessionLog.WriteInfo(output => report.WriteReport(output));
        }
    }
}
