using System;
using System.Collections.Generic;

namespace Cone.Core
{
    public interface IConeFixture : ITestInterceptor
    {
        object Fixture { get; }
        Type FixtureType { get; }
		IEnumerable<string> Categories { get; }
    }
}
