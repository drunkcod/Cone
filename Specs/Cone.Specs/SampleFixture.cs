using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cone
{
	class SampleFixture
	{
		[BeforeAll]
		public void BeforeAll() {}

		[BeforeEach]
		public void BeforeEach() {}

		public void Test() {}

		public async Task TestAsync() { await Task.FromResult(0); }

		public int Uninteresting() { return 42; }

		[Row(42)]
		public void RowTest(int input) {}

		[AfterEach]
		public void AfterEach() {}

		[AfterEach]
		public void AfterEachWithResult(ITestResult testResult) {}

		[AfterAll]
		public void AfterAll() {}

		public IEnumerable<IRowTestData> RowSource(){ return null; }
	}
}
