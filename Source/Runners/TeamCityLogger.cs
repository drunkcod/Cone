using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cone.Core;

namespace Cone.Runners
{
	public class TeamCityLogger : ISessionLogger, ISuiteLogger, ITestLogger
	{
		readonly TextWriter output;
		IConeSuite activeSuite;
		IConeTest activeTest;

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
			activeSuite = suite;
			WriteLine("##teamcity[testSuiteStarted name='{0}']", activeSuite.Name);
			return this; 
		}

		void ISessionLogger.EndSession() { }

		ITestLogger ISuiteLogger.BeginTest(IConeTest test) {
			activeTest = test;
			WriteLine("##teamcity[testStarted name='{0}']", activeTest.TestName.Name);
			return this;
		}

		void ISuiteLogger.EndSuite() {
			WriteLine("##teamcity[testSuiteFinished name='{0}']", activeSuite.Name);
			activeSuite = null;
		}

		void ITestLogger.Failure(ConeTestFailure testFailure) {
			foreach(var failure in testFailure.Errors) {
			Maybe.Map(failure.Actual, failure.Expected, (actual, expected) => new { actual, expected })
				.Do( x => WriteLine("##teamcity[testFailed type='comparisionFailure' name='{0}' message='{1}' details='{2}'] actual='{3}' expected='{4}'", activeTest.TestName.Name, failure.Message, failure, x.actual, x.expected),
					() => WriteLine("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", activeTest.TestName.Name, failure.Message, failure));
			}			
		}

		void ITestLogger.Success() { }

		void ITestLogger.Pending(string reason) {
			WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", activeTest.TestName.Name, reason);
		}

		void ITestLogger.Skipped() { }

		public void EndTest() { 
			WriteLine("##teamcity[testFinished name='{0}']", activeTest.TestName.Name);
		}

		void WriteLine(string format, params object[] args) {
			output.WriteLine(format, Array.ConvertAll(args, x => Escape((x ?? string.Empty).ToString())));
		}

		string Escape(string input) {
			return input
				.Replace("|", "||")
				.Replace("'", "|'")
				.Replace("\n", "|n")
				.Replace("\r", "|r")
				.Replace("]", "|]");
		}
	}
}
