using System;
using NUnit.Core;
using System.Reflection;

namespace Cone.Addin
{
	public class NUnitTestResultAdapter : ITestResult
	{
		readonly TestResult result;

		public NUnitTestResultAdapter(TestResult result) {
			this.result = result;
		}

		public TestName TestName {
			get { return result.Test.TestName; }
		}

		TestStatus ITestResult.Status {
			get {
				switch (result.ResultState) {
					case ResultState.Ignored: return TestStatus.Pending;
					case ResultState.Failure: 
						switch(result.FailureSite) {
							case FailureSite.SetUp: return TestStatus.SetupFailure;
							default: return TestStatus.TestFailure;
						}
					default: return TestStatus.Success;
				}
			}
		}
		
		ITestName ITestResult.TestName { get { return new NUnitTestNameAdapter(TestName); } }


		void ITestResult.Begin() { }
		void ITestResult.Success() { result.Success(); }
		void ITestResult.Pending(string reason) { result.Ignore(reason); }
		void ITestResult.BeforeFailure(Exception ex) { Failure(ex, FailureSite.SetUp); }
		void ITestResult.TestFailure(Exception ex) { Failure(ex, FailureSite.Test); }
		void ITestResult.AfterFailure(Exception ex) { Failure(ex, FailureSite.TearDown); }

		void Failure(Exception ex, FailureSite site) {
			var invocationException = ex as TargetInvocationException;
			if(invocationException != null)
				ex = invocationException.InnerException;
			result.SetResult(ResultState.Failure, ex.Message, ex.StackTrace, site);
		}

	}
}