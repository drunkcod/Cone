using System;
using System.IO;
using Cone.Core;
using System.Threading;
using System.Diagnostics;

namespace Cone.Runners
{
	public class TeamCityLogger : ISessionLogger
	{
		readonly TextWriter output;

		class TeamCitySuiteLogger : ISuiteLogger, ITestLogger
		{ 
			readonly TeamCityLogger parent;
			readonly IConeSuite activeSuite;
			readonly int flowId;
			readonly Stopwatch duration = new Stopwatch();

			IConeTest activeTest;

			public TeamCitySuiteLogger(TeamCityLogger parent, IConeSuite suite, int flowId) {
				this.parent = parent;
				this.activeSuite = suite;
				this.flowId = flowId;
			}

			ITestLogger ISuiteLogger.BeginTest(IConeTest test) {
				activeTest = test;
				return this;
			}

			void ISuiteLogger.EndSuite() => 
				WriteLine("testSuiteFinished name='{0}'", activeSuite.Name);

			void ITestLogger.Failure(ConeTestFailure testFailure) {
				foreach(var failure in testFailure.Errors) {
				Maybe.Map(failure.Actual, failure.Expected, (actual, expected) => new { actual, expected })
					.Do( x => WriteLine("testFailed type='comparisionFailure' name='{0}' message='{1}' details='{2}'] actual='{3}' expected='{4}'", activeTest.TestName.Name, failure.Message, failure, x.actual, x.expected),
						() => WriteLine("testFailed name='{0}' message='{1}' details='{2}'", activeTest.TestName.Name, failure.Message, failure));
				}			
			}

			void ITestLogger.Success() { }

			void ITestLogger.Pending(string reason) =>
				WriteLine("testIgnored name='{0}' message='{1}'", activeTest.TestName.Name, reason);

			void ITestLogger.Skipped() { }

			void ITestLogger.TestStarted() {
				WriteLine("testStarted name='{0}'", activeTest.TestName.Name);
				duration.Restart();
			}

			void ITestLogger.TestFinished() {
				duration.Stop();
				WriteLine("testFinished name='{0}' duration='{1}'", activeTest.TestName.Name, duration.ElapsedMilliseconds);
			}

			public void WriteLine(string format, params object[] args) => 
				parent.WriteLine($"##teamcity[{format} flowId='{flowId}']", args);
		}

		public TeamCityLogger(TextWriter output) {
			this.output = output;
		}

		void ISessionLogger.WriteInfo(Action<ISessionWriter> output) {
			var message = new StringWriter();
			output(new TextSessionWriter(message));
			WriteLine("##teamcity[message text='{0}' status='NORMAL']", message);
		}

		void ISessionLogger.BeginSession() { }

		ISuiteLogger ISessionLogger.BeginSuite(IConeSuite suite) {
			var logger = new TeamCitySuiteLogger(this, suite, Thread.CurrentThread.ManagedThreadId);
			logger.WriteLine("testSuiteStarted name='{0}'", suite.Name);
			return logger; 
		}

		void ISessionLogger.EndSession() { }

		void WriteLine(string format, params object[] args) =>
			output.WriteLine(format, Array.ConvertAll(args, x => Escape((x ?? string.Empty).ToString())));

		static string Escape(string input) {
			return input
				.Replace("|", "||")
				.Replace("'", "|'")
				.Replace("\n", "|n")
				.Replace("\r", "|r")
				.Replace("[", "|[")
				.Replace("]", "|]");
		}
	}
}
