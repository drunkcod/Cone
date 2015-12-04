namespace Cone.Core
{
	public interface ITestExecutionContext 
	{
		TestContextStep Establish(IFixtureContext context, TestContextStep next);
	}
}