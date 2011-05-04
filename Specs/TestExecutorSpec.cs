using System;
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

            contextMock.Setup(x => x.FixtureType).Returns(typeof(object));

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

        public void run_after_when_before_throws() {
            contextMock.Setup(x => x.Before()).Throws(new NotImplementedException());

            RunTest();

            contextMock.Verify(x => x.Before());
            testMock.Verify(x => x.Run(TestResult), Times.Never());
            contextMock.Verify(x => x.After(TestResult));
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

        public void report_AfterFailure_if_excpetion_raished_during_cleanup() { 
            var ex = new NotImplementedException();
            contextMock.Setup(x => x.After(TestResult)).Throws(ex);

            RunTest();

            testResultMock.Verify(x => x.AfterFailure(ex));     
        }

        public void doesnt_Run_test_when_establish_context_fails() {
            var ex = new NotImplementedException();
            contextMock.Setup(x => x.Before()).Throws(ex);

            RunTest();

            testMock.Verify(x => x.Run(TestResult), Times.Never());     
        }

        [Context("when fixture contains interceptor")]
        public class FixtureWithRules 
        {
            public class MyInterceptor : ITestInterceptor
            {
                public int BeforeCalls = 0;
                public int AfterCalls = 0;

                public void Before() { ++BeforeCalls; }

                public void After(ITestResult result) { ++AfterCalls; }
            }

            public MyInterceptor Interceptor = new MyInterceptor();

            public void before_is_called() {
                Verify.That(() => Interceptor.BeforeCalls == 1);
            }

            public void zzz_after_is_called() {
                Verify.That(() => Interceptor.AfterCalls == 1);
            }
        }

        void RunTest() { testExecutor.Run(testMock.Object, TestResult); }
    }
}
