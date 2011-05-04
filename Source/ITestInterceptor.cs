namespace Cone
{
    public interface ITestInterceptor
    {
        void Before();
        void After(ITestResult result);
    }
}
