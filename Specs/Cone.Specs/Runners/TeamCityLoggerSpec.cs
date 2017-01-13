using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cone.Core;
using Cone.Stubs;

namespace Cone.Runners
{
	//officail spec found at: http://confluence.jetbrains.net/display/TCD4/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingTests
	[Describe(typeof(TeamCityLogger))]
	public class TeamCityLoggerSpec
	{
		ISessionLogger Logger;
		StringBuilder Output;
		string[] Result { get { return Output.ToString().Split(new[]{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); } }

		[BeforeEach]
		public void CreateLogger() {
			Output = new StringBuilder();
			Logger = new TeamCityLogger(new StringWriter(Output));
		}

		[DisplayAs("information message: ##teamcity[message text='<message text>' status='NORMAL']")]
		public void information_messages() {
			Logger.WriteInfo(output => output.Write("Hello World!"));
			Check.That(() => Result.Last() == "##teamcity[message text='Hello World!' status='NORMAL']");
		} 

		[DisplayAs("suite starting: ##teamcity[testSuiteStarted name='suite.name']")]
		public void suite_starting() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"));
			Check.That(() => Result.Last().StartsWith("##teamcity[testSuiteStarted name='Namespace.SuiteName'"));
		}

		[DisplayAs("suite finished: ##teamcity[testSuiteFinished name='suite.name']")]
		public void suite_finished() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.EndSuite();

			Check.That(() => Result.Last().StartsWith("##teamcity[testSuiteFinished name='Namespace.SuiteName'"));
		}

		[DisplayAs("test started: ##teamcity[testStarted name='testname']")]
		public void test_started() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.BeginTest(Test().InContext("Namespace.SuiteName").WithName("MyTest"))
				.TestStarted();

			Check.That(() => Result.Last().StartsWith("##teamcity[testStarted name='MyTest'"));		
		}

		[DisplayAs("test finished: ##teamcity[testFinished name='testname']")]
		public void test_finished() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(Test().InContext("Namespace.SuiteName").WithName("MyTest"), log => log.Success());
			Check.That(() => Result.Last().StartsWith("##teamcity[testFinished name='MyTest'"));
		}

		[DisplayAs("test ignored: ##teamcity[testIgnored message='ignore comment']")]
		public void test_ignored() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(Test().InContext("Namespace.SuiteName").WithName("MyTest"), log => log.Pending("Teh Reason!"));
			Check.That(() => Result.Any(line => line.StartsWith("##teamcity[testIgnored name='MyTest' message='Teh Reason!'")));
			Check.That(() => Result.Last().StartsWith("##teamcity[testFinished name='MyTest'"));
		}

		[DisplayAs("test failed: ##teamcity[testFailed name='testname' message='failure message' details='message and stack trace'")]
		public void test_failed() {
			var test = Test().InContext("Namespace.SuiteName").WithName("MyTest");
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(test, log => log.Failure(new ConeTestFailure(test.TestName, new Exception("Teh Error!"), FailureType.Test)));
			//Check.That(() => Result.Any(line => line == "##teamcity[testFailed name='MyTest' message='Teh Error!' details='Namespace.SuiteName.MyTest: Teh Error!']"));
			Check.That(() => Result.Last().StartsWith("##teamcity[testFinished name='MyTest'"));
		}

		[DisplayAs("test failed: ##teamcity[testFailed type='comparisionFailure' name='testname' message='failure message' details='message and stack trace'")]
		public void comparision_test_failed() {
			var test = Test().InContext("Namespace.SuiteName").WithName("MyTest");
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(test, log => log.Failure(new ConeTestFailure(test.TestName, new CheckFailed(string.Empty, new[]{ new FailedExpectation("Teh Error!", Maybe<object>.Some(1), Maybe<object>.Some(2)) }, null), FailureType.Test)));		
			//Check.That(() => Result.Any(line => line == "##teamcity[testFailed type='comparisionFailure' name='MyTest' message='Teh Error!' details='Namespace.SuiteName.MyTest: Teh Error!'] actual='1' expected='2'"));
			Check.That(() => Result.Last().StartsWith("##teamcity[testFinished name='MyTest'"));
		}

		public void escapes_values() {
			Logger.WriteInfo(output => output.Write("'\n\r|[]"));
			Check.That(() => Result.Last() == "##teamcity[message text='|'|n|r|||[|]' status='NORMAL']");
		}

		ConeSuiteStub Suite() {
			return new ConeSuiteStub();
		}

		ConeTestStub Test() {
			return new ConeTestStub();
		}
	}
}
