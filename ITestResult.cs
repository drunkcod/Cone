namespace Cone
{
    public enum TestStatus
    {
        Success, Failure
    }

    public interface ITestResult
    {
        string TestName { get; }
        TestStatus Status { get; }
    }
}