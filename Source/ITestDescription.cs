using System.Collections.Generic;

namespace Cone
{
    public interface IFixtureDescription
    {
        IEnumerable<string> Categories { get; }
        string SuiteName { get; }
        string SuiteType { get; }
        string TestName { get; }
    }
}
