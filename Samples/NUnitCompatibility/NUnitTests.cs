using System.Collections.Generic;
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

		[TestCase(1, 2, Result = 3)
		,TestCase(1, 1, Result = 5, TestName = "1 + 1 = 5")
		,TestCaseSource("AddTestCaseSource")]
		public int Add(int a, int b) { return a + b; } 

		public IEnumerable<TestCaseData> AddTestCaseSource() {
			yield return new TestCaseData(3, 4).Returns(7).SetName("one + one == two");
		}
	}
}
