using System;

namespace Cone.Core
{
	public interface ITestResult
	{
		ITestName TestName { get; }
		TestStatus Status { get; }

		void Begin();
		void Success();
		void Pending(string reason);
		void BeforeFailure(Exception ex);
		void TestFailure(Exception ex);
		void AfterFailure(Exception ex);
	}
}