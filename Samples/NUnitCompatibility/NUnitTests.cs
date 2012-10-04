using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	}
}
