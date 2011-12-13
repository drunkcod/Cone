using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    class SampleFixture
    {
        [BeforeAll]
        public void BeforeAll() { }

        [BeforeEach]
        public void BeforeEach() { }

        public void Test(){}

        [Row(42)]
        public void RowTest(int input){ }

        [AfterEach]
        public void AfterEach() { }

        [AfterEach]
        public void AfterEachWithResult(ITestResult testResult) {}

        [AfterAll]
        public void AfterAll() { }

        public IEnumerable<IRowTestData> RowSource(){ return null; }
    }
}
