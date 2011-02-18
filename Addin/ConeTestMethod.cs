using NUnit.Core;

namespace Cone.Addin
{
    class ConeTestMethod : ConeTest
    {
        readonly ConeMethodThunk thunk;

        public ConeTestMethod(ConeMethodThunk thunk, Test suite, TestExecutor testExecutor, string name)
            : base(suite, testExecutor, name) {
            this.thunk = thunk;
        }
       
        public override void Run(ITestResult testResult) { thunk.Invoke(Fixture, null); }
    }
}
