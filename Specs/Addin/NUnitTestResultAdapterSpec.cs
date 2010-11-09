using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Core;

namespace Cone.Addin
{
    [Describe(typeof(NUnitTestResultAdapter))]
    public class NUnitTestResultAdapterSpec
    {
        TestResult TestResult = new TestResult(new TestName() { FullName = "Context.TestName", Name = "TestName" });

        public void should_expose_TestName() {
            ITestResult adapter = new NUnitTestResultAdapter(TestResult);
            Verify.That(() => adapter.TestName.Name == "TestName");
        }

        public void should_expose_Context() {
            ITestResult adapter = new NUnitTestResultAdapter(TestResult);
            Verify.That(() => adapter.TestName.FullName == "Context.TestName");
        }
    }
}
