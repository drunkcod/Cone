using System;
using System.Reflection;

namespace Cone.Core
{
    public interface IFixtureHolder
    {
        Type FixtureType { get; }
        object Fixture { get; set; }
        MethodInfo[] SetupMethods { get; }
        MethodInfo[] TeardownMethods { get; }
        MethodInfo[] AfterEachWithResult { get; }
    }
}
