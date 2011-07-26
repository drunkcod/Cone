using System.Reflection;

namespace Cone.Core
{
    public interface IFixtureHolder
    {
        object Fixture { get; set; }
        MethodInfo[] FixtureSetupMethods { get; }
        MethodInfo[] FixtureTeardownMethods { get; }
        MethodInfo[] SetupMethods { get; }
        MethodInfo[] TeardownMethods { get; }
        MethodInfo[] AfterEachWithResult { get; }
    }
}
