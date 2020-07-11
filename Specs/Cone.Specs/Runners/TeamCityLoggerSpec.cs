using System;
using System.IO;
using System.Linq;
using System.Text;
using CheckThat;
using CheckThat.Expectations;
using CheckThat.Internals;
using Cone.Core;
using Cone.Stubs;

namespace Cone.Runners
{
	//officail spec found at: http://confluence.jetbrains.net/display/TCD4/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-ReportingTests
	[Describe(typeof(TeamCityLogger))]
	public class TeamCityLoggerSpec
	{
		const int FlowId = 1;
		ISessionLogger Logger;
		StringBuilder Output;
		string[] Result { get { return Output.ToString().Split(new []{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); } }

		[BeforeEach]
		public void CreateLogger() {
			Output = new StringBuilder();
			Logger = new TeamCityLogger(new StringWriter(Output), () => FlowId);
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
			Check.That(() => Result[Result.Length - 2].StartsWith("##teamcity[testFailed name='MyTest' message='Teh Error!' details='Namespace.SuiteName.MyTest:|n→ Teh Error!|n'"));
			Check.That(() => Result.Last().StartsWith("##teamcity[testFinished name='MyTest'"));
		}

		[DisplayAs("test failed: ##teamcity[testFailed type='comparisionFailure' name='testname' message='failure message' details='message and stack trace'")]
		public void comparision_test_failed() {
			var test = Test().InContext("Namespace.SuiteName").WithName("MyTest");
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(test, log => log.Failure(new ConeTestFailure(test.TestName, new CheckFailed(string.Empty, new []{ new FailedExpectation(ConeMessage.Parse("Teh Error!"), Maybe<object>.Some(1), Maybe<object>.Some(2)) }, null), FailureType.Test)));
			Check.That(() => Result[Result.Length - 2].StartsWith("##teamcity[testFailed type='comparisionFailure' name='MyTest' message='Teh Error!' details='Namespace.SuiteName.MyTest:|n→ Teh Error!|n' actual='1' expected='2'"));
			Check.That(() => Result.Last().StartsWith("##teamcity[testFinished name='MyTest'"));
		}

		public void prints_only_single_failure_row() {
			var test = Test().InContext("Namespace.SuiteName").WithName("MyTest");
			Logger.BeginSuite(Suite().WithName("Namespace.SuiteName"))
				.WithTestLog(test, log => log.Failure(new ConeTestFailure(test.TestName, new CheckFailed(string.Empty, new[] {
					new FailedExpectation(ConeMessage.Parse("Error 1"), Maybe<object>.Some(1), Maybe<object>.Some(2)),
					new FailedExpectation(ConeMessage.Parse("Error 2"), Maybe<object>.Some(1), Maybe<object>.Some(2)),
				}, null), FailureType.Test)));
			Check.With(() => Result).That(
				r => r.Length == 3,
				r => r.Count(x => x.Contains("testFailed")) == 1,
				r => r.Reverse().Skip(1).First() == $"##teamcity[testFailed name='MyTest' message='Error 1|nError 2' details='Namespace.SuiteName.MyTest:|n→ Error 1|n→ Error 2|n' flowId='{FlowId}']");
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
