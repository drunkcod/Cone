namespace Cone.Core
{
	public interface IFixtureContext
	{
		IConeAttributeProvider Attributes { get; }
		IConeFixture Fixture { get; }
	}
}