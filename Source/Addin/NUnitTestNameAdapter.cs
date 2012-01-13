using NUnit.Core;

namespace Cone.Addin
{
    public class NUnitTestNameAdapter : ITestName
    {
        readonly TestName testName;

        public NUnitTestNameAdapter(TestName testName) {
            this.testName = testName;
        }

        public string Context {
            get { return testName.FullName.Substring(0, testName.FullName.LastIndexOf(".")); }
        }

        public string Name { get { return testName.Name; } }

        public string FullName { get { return testName.FullName; } }
    }
}
