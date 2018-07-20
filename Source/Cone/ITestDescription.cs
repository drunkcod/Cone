using System.Collections.Generic;

namespace Cone
{
    public interface IFixtureDescription : IHaveCategories
    {
        string SuiteName { get; }
        string SuiteType { get; }
        string TestName { get; }
    }
}
