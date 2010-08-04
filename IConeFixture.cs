namespace Cone
{
    public interface IConeFixture
    {
        object Fixture { get; }
        void Before();
        void After(ITestResult testResult);
    }
}
