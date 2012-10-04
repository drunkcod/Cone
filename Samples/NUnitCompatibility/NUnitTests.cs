using NUnit.Framework;

namespace Cone.Samples.NUnitCompatibility
{
	[TestFixture]
	public class NUnitTests
	{
		public bool SetUpCalled;

		[SetUp]
		public void SetUp() { SetUpCalled = true; }

		[TearDown]
		public void TearDown() { }

		[TestFixtureSetUp]
		public void FixtureSetUp() { }

		[TestFixtureTearDown]
		public void FixtureTearDown() { }

		public void MyTest() { Assert.That(SetUpCalled);}

		[TestCase(1, 2, Result = 3)]
		[TestCase(1, 1, Result = 5, TestName = "1 + 1 = 5")]
		public int Add(int a, int b) { return a + b; } 
	}
}
