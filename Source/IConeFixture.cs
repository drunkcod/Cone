using System;

namespace Cone
{
    public interface IConeFixture
    {
        Type FixtureType { get; }
        void Before();
        void After(ITestResult testResult);
    }
}
