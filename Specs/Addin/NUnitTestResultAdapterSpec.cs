using System;
using NUnit.Core;

namespace Cone.Addin
{
    [Describe(typeof(NUnitTestResultAdapter))]
    public class NUnitTestResultAdapterSpec
    {
        TestResult TestResult;

        [BeforeEach]
        public void given_a_sample_test_result() {
            TestResult = new TestResult(new TestName() { FullName = "Context.TestName", Name = "TestName" });
        }

        public void should_expose_TestName() {
            var adapter = new NUnitTestResultAdapter(TestResult);
            Verify.That(() => adapter.TestName.Name == "TestName");
        }

        public void should_expose_Context() {
            var adapter = new NUnitTestResultAdapter(TestResult);
            Verify.That(() => adapter.TestName.FullName == "Context.TestName");
        }

        public void reports_setup_failure() {
            ITestResult adapter = new NUnitTestResultAdapter(TestResult);
            adapter.BeforeFailure(new Exception());
            Verify.That(() => adapter.Status == TestStatus.SetupFailure);
        }
    }
}
