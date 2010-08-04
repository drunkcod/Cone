using System;

namespace Cone
{
    public enum TestStatus
    {
        Success, Pending, Failure
    }

    public interface ITestResult
    {
        string TestName { get; }
        TestStatus Status { get; }

        void Success();
        void Pending(string reason);
        void TestFailure(Exception error);
    }
}