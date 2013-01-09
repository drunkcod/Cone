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
			Verify.That(() => Result.Last() == "##teamcity[message text='Hello World!' status='NORMAL']");
		} 

		[DisplayAs("suite starting: ##teamcity[testSuiteStarted name='suite.name']")]
		public void suite_starting() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"));
			Verify.That(() => Result.Last() == "##teamcity[testSuiteStarted name='Namespace.SuiteName']");
		}

		[DisplayAs("suite finished: ##teamcity[testSuiteFinished name='suite.name']")]
		public void suite_finished() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.EndSuite();

			Verify.That(() => Result.Last() == "##teamcity[testSuiteFinished name='Namespace.SuiteName']");
		}

		[DisplayAs("test started: ##teamcity[testStarted name='testname']")]
		public void test_started() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.BeginTest(Test().InContext("Namespace.SuiteName").WithName("MyTest"));

			Verify.That(() => Result.Last() == "##teamcity[testStarted name='MyTest']");		
		}

		[DisplayAs("test finished: ##teamcity[testFinished name='testname']")]
		public void test_finished() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(Test().InContext("Namespace.SuiteName").WithName("MyTest"), log => log.Success());
			Verify.That(() => Result.Last() == "##teamcity[testFinished name='MyTest']");
		}

		[DisplayAs("test ignored: ##teamcity[testIgnored message='ignore comment']")]
		public void test_ignored() {
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(Test().InContext("Namespace.SuiteName").WithName("MyTest"), log => log.Pending("Teh Reason!"));
			Verify.That(() => Result.Any(line => line == "##teamcity[testIgnored name='MyTest' message='Teh Reason!']"));
			Verify.That(() => Result.Last() == "##teamcity[testFinished name='MyTest']");
		}

		[DisplayAs("test failed: ##teamcity[testFailed name='testname' message='failure message' details='message and stack trace'")]
		public void test_failed() {
			var test = Test().InContext("Namespace.SuiteName").WithName("MyTest");
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(test, log => log.Failure(new ConeTestFailure(test.TestName, new Exception("Teh Error!"), FailureType.Test)));
			Verify.That(() => Result.Any(line => line == "##teamcity[testFailed name='MyTest' message='Teh Error!' details='Namespace.SuiteName.MyTest: Teh Error!']"));
			Verify.That(() => Result.Last() == "##teamcity[testFinished name='MyTest']");
		}

		[DisplayAs("test failed: ##teamcity[testFailed type='comparisionFailure' name='testname' message='failure message' details='message and stack trace'")]
		public void comparision_test_failed() {
			var test = Test().InContext("Namespace.SuiteName").WithName("MyTest");
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(test, log => log.Failure(new ConeTestFailure(test.TestName, new ExpectationFailedException("Teh Error!", Maybe<object>.Some(1), Maybe<object>.Some(2)), FailureType.Test)));		
			Verify.That(() => Result.Any(line => line == "##teamcity[testFailed type='comparisionFailure' name='MyTest' message='Teh Error!' details='Namespace.SuiteName.MyTest: Teh Error!'] actual='1' expected='2'"));
			Verify.That(() => Result.Last() == "##teamcity[testFinished name='MyTest']");
		}

		public void escapes_values() {
			Logger.WriteInfo(output => output.Write("'\n\r|]"));
			Verify.That(() => Result.Last() == "##teamcity[message text='|'|n|r|||]' status='NORMAL']");
		}

		ConeSuiteStub Suite() {
			return new ConeSuiteStub();
		}

		ConeTestStub Test() {
			return new ConeTestStub();
		}
	}
}
