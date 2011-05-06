using System;

namespace Cone.Core
{
    public interface IConeFixture : ITestInterceptor
    {
        object Fixture { get; }
        Type FixtureType { get; }
    }
}
