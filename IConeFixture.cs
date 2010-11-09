namespace Cone
{
    public interface IConeFixture
    {
        void Before();
        void After(ITestResult testResult);
    }
}
