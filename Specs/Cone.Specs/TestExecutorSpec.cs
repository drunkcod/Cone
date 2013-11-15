using System;
using System.Reflection;
using System.Text;
using Moq;
using System.Collections.Generic;

namespace Cone.Core
{
    [Describe(typeof(TestExecutor))]
    public class TestExecutorSpec
    {
        Mock<IConeFixture> contextMock;
        Mock<ITestResult> testResultMock;
        Mock<IConeTest> testMock;
        TestExecutor testExecutor;

        ITestResult TestResult { get { return testResultMock.Object; } }

		class NullAttributeProvider : IConeAttributeProvider
		{
			public IEnumerable<object> GetCustomAttributes(Type attributeType) { return new object[0]; }
		}

        [BeforeEach]
        public void CreateMocks() {
            contextMock = new Mock<IConeFixture>();

            contextMock.Setup(x => x.FixtureType).Returns(typeof(object));

            testResultMock = new Mock<ITestResult>();
            testMock = new Mock<IConeTest>();
			testMock.SetupGet(x => x.Attributes).Returns(new NullAttributeProvider());
            testExecutor = new TestExecutor(contextMock.Object);
        }

        public void happy_path_is_Before_Run_After() {
            RunTest();

            contextMock.Verify(x => x.Before());
            testMock.Verify(x => x.Run(TestResult));
            contextMock.Verify(x => x.After(TestResult));
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
        public class FixtureWithInterceptor 
        {
            StringBuilder ExecutionSequence = new StringBuilder();

            public class MyInterceptor : ITestContext
            {
                readonly StringBuilder target;

                public MyInterceptor(StringBuilder target) {
                    this.target = target;
                }

                public void Before() { target.Append("->Interceptor.Before"); }

                public void After(ITestResult result) { target.Append("->Interceptor.After"); }
            }

            public MyInterceptor Interceptor;

            [BeforeAll]
            public void EstablishContext() {
                ExecutionSequence = new StringBuilder();
                Interceptor = new MyInterceptor(ExecutionSequence);
            }

            [BeforeEach]
            public void Before() { ExecutionSequence.Append("->Test.Before"); }

            [AfterEach]
            public void After() { ExecutionSequence.Append("->Test.After"); }

            public void before_sequence() {
                ExecutionSequence.Append("->Test");
                Check.That(() => ExecutionSequence.ToString() == "->Interceptor.Before->Test.Before->Test");
            }

            public void zzz_after_sequence() {
                Check.That(() => ExecutionSequence.ToString().StartsWith("->Interceptor.Before->Test.Before->Test->Test.After->Interceptor.After"));
            }
        }

        void RunTest() { testExecutor.Run(testMock.Object, TestResult); }
    }
}
