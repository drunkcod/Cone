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

		void ISessionLogger.WriteInfo(Action<TextWriter> output) {
			var message = new StringWriter();
			output(message);
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

		void ISuiteLogger.Done() {
			WriteLine("##teamcity[testSuiteFinished name='{0}']", activeSuite.Name);
			activeSuite = null;
		}

		void ITestLogger.Failure(ConeTestFailure failure) {
			WriteLine("##teamcity[testFailed name='{0}' message='{1}' details='{2}']", activeTest.TestName.Name, failure.Message, failure);
			TestDone();
		}

		void ITestLogger.Success() {
			TestDone();
		}

		void ITestLogger.Pending(string reason) {
			WriteLine("##teamcity[testIgnored name='{0}' message='{1}']", activeTest.TestName.Name, reason);
			TestDone();
		}

		void ITestLogger.Skipped() { }

		void TestDone() { 
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
