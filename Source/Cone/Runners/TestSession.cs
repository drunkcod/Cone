using Cone.Core;
using System;
using System.Collections.Generic;

namespace Cone.Runners
{
	public class TestSession
	{
		class TestResult : ITestResult
		{
			readonly IConeTest test;
			readonly ITestLogger log;

			public TestResult(IConeTest test, ITestLogger log) {
				this.test = test;
				this.log = log;
			}

			public ITestName TestName { get { return test.TestName; } }
			public TestStatus Status { get; private set; }

			void ITestResult.Begin() {
				Status = TestStatus.Running;
				log.TestStarted();
			}

			void ITestResult.Success() { 
				Status = TestStatus.Success;
				log.Success();
			}

			void ITestResult.Pending(string reason) {
				Status = TestStatus.Pending;
				log.Pending(reason);
			}
			
			void ITestResult.BeforeFailure(Exception ex) {
				Fail(TestStatus.SetupFailure, FailureType.Setup, ex);
			}

			void ITestResult.TestFailure(Exception ex) {
				Fail(TestStatus.TestFailure, FailureType.Test, ex);
			}
			
			void ITestResult.AfterFailure(Exception ex) {
				Fail(TestStatus.TeardownFailure, FailureType.Teardown, ex);
			}

			void Fail(TestStatus status, FailureType failureType, Exception ex) {
				Status = status;
				var fixtureFailure = ex as FixtureException;
				if(fixtureFailure != null)
					for(var i = 0; i != fixtureFailure.Count; ++i)
						log.Failure(new ConeTestFailure(test.TestName, fixtureFailure[i], failureType));
				else
					log.Failure(new ConeTestFailure(test.TestName, ex, failureType));
			}
		}

		readonly ISessionLogger sessionLog;
		readonly TestSessionReport report = new TestSessionReport();

		public TestSession(ISessionLogger sessionLog) {
			this.sessionLog = new MulticastSessionLogger(sessionLog, report);
		}

		public Predicate<IConeTest> ShouldSkipTest = _ => false; 
		public Predicate<IConeSuite> IncludeSuite = _ => true;
		public Func<IConeFixture, ITestExecutor> GetTestExecutor = x => new TestExecutor(x);

		public int FailureCount => report.Failed;

		public void RunSession(Action<Action<ConeSuite>> @do) {
			sessionLog.BeginSession();
			@do(CollectSuite);
			sessionLog.EndSession();
		}

		public void RunTests(IEnumerable<IConeTest> tests) {
			sessionLog.BeginSession();
			var singleTest = new IConeTest[1];
			foreach (var item in tests){
				var log = sessionLog.BeginSuite(item.Suite);
				singleTest[0] = item;
				CollectResults(singleTest, item.Suite.Fixture, log);
				log.EndSuite();
			}
			sessionLog.EndSession();
		}

		void CollectSuite(ConeSuite suite) {
			var log = sessionLog.BeginSuite(suite);
			suite.Run((tests, fixture) => CollectResults(tests, fixture, log));
			log.EndSuite();
		}

		void CollectResults(IEnumerable<IConeTest> tests, IConeFixture fixture, ISuiteLogger suiteLog) {
			var testExecutor = GetTestExecutor(fixture);
			var beforeFailure = new Lazy<Exception>(() => Initiaize(testExecutor));
			tests.ForEach(test =>
				suiteLog.WithTestLog(test, log => {
					if(ShouldSkipTest(test))
						log.Skipped();
					else {
						ITestResult result = new TestResult(test, log);
						if(beforeFailure.Value != null)
							result.BeforeFailure(beforeFailure.Value);
						else
							testExecutor.Run(test, result);
					}
				}));

			try {
				testExecutor.Relase();
			} catch { }
		}

		static Exception Initiaize(ITestExecutor executor) {
			try {
				executor.Initialize();
				return null;
			} catch(Exception ex) {
				return ex;
			}
		}

		public void Report() {
			sessionLog.WriteInfo(output => report.WriteReport(output));
		}
	}
}
