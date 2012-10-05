using System.Reflection;
using Cone.Core;
using NUnit.Core;

namespace Cone.Addin
{
    class AddinTestMethod : AddinTest
    {
        readonly ConeMethodThunk thunk;

        public AddinTestMethod(ConeMethodThunk thunk, Test suite, TestExecutor testExecutor, string name)
            : base(suite, testExecutor, name) {
            this.thunk = thunk;
        }

		public override IConeAttributeProvider Attributes { get { return thunk; } }

        public override void Run(ITestResult testResult) { thunk.Invoke(Fixture, null); }
    }
}
