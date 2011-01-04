namespace Cone
{
    public interface IFixtureDescription
    {
        string Category { get; }
        string SuiteName { get; }
        string TestName { get; }
    }
}
