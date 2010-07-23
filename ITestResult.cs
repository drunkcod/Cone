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
    }
}