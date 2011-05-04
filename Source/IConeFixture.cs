using System;

namespace Cone
{
    public interface IConeFixture : ITestInterceptor
    {
        object Fixture { get; }
        Type FixtureType { get; }
    }
}
