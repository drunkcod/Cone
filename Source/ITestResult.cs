using System;

namespace Cone
{
    public interface ITestResult
    {
		ITestName TestName { get; }
		TestStatus Status { get; }

        void Success();
        void Pending(string reason);
        void BeforeFailure(Exception ex);
        void TestFailure(Exception ex);
        void AfterFailure(Exception ex);
    }
}