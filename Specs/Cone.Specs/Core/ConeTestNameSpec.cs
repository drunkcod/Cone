
namespace Cone.Core
{
	[Describe(typeof(ConeTestName))]
	public class ConeTestNameSpec
	{
		public void trims_leading_whitespace() {
			Check.That(() => new ConeTestName("<context>", " <name>").Name == "<name>");
		}
	}
}
