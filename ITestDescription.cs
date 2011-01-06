namespace Cone
{
    public interface IFixtureDescription
    {
        string Category { get; }
        string SuiteName { get; }
        string SuiteType { get; }
        string TestName { get; }
    }
}
