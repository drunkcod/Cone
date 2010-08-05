using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;

namespace Cone
{
    [Describe(typeof(TestExecutor))]
    public class TestExecutorSpec
    {
        Mock<IConeFixture> contextMock;
        Mock<ITestResult> testResultMock;
        Mock<IConeTest> testMock;
        TestExecutor testExecutor;

        ITestResult TestResult { get { return testResultMock.Object; } }

        [BeforeEach]
        public void CreateMocks() {
            contextMock = new Mock<IConeFixture>();
            testResultMock = new Mock<ITestResult>();
            testMock = new Mock<IConeTest>();
            testExecutor = new TestExecutor(contextMock.Object);
        }

        public void happy_path_is_Before_Run_After() {
            RunTest();

            contextMock.Verify(x => x.Before());
            testMock.Verify(x => x.Run(TestResult));
            contextMock.Verify(x => x.After(TestResult));
            testResultMock.Verify(x => x.Success());
        }

        public void report_BeforeFailure_if_exception_raised_when_establishing_context() {
            var ex = new NotImplementedException();
            contextMock.Setup(x => x.Before()).Throws(ex);

            RunTest();

            testResultMock.Verify(x => x.BeforeFailure(ex));     
        }

        public void report_TestFailure_if_expcetion_raised_by_test() {
            var ex = new NotImplementedException();
            testMock.Setup(x => x.Run(TestResult)).Throws(ex);

            RunTest();

            testResultMock.Verify(x => x.TestFailure(ex));        
        }

        public void doesnt_Run_test_when_establish_context_fails() {
            var ex = new NotImplementedException();
            contextMock.Setup(x => x.Before()).Throws(ex);

            RunTest();

            testMock.Verify(x => x.Run(TestResult), Times.Never());     
        }

        void RunTest() { testExecutor.Run(testMock.Object, TestResult); }
    }
}
