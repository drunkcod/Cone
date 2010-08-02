namespace Cone
{
    public interface IConeTest
    {
        void Before();
        void After(ITestResult testResult);
    }
}
