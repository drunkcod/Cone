using System.Reflection;
using System.Threading;
using Cone.Core;
using NUnit.Core;

namespace Cone.Addin
{
    public abstract class ConeTest : Test, IConeTest
    {
        internal readonly TestExecutor testExecutor;

        static int id = 0;
        static TestName BuildTestName(Test suite, string name) {
            var testId = Interlocked.Increment(ref id);
            var testName = new TestName {
                FullName = suite.TestName.FullName + "." + name,
                Name = name,
                TestID = new TestID(testId) 
            };
            return testName;
        }

        protected ConeTest(Test suite, TestExecutor testExecutor, string name): base(BuildTestName(suite, name)) {
            Parent = suite;
            this.testExecutor = testExecutor; 
        }

        public override object Fixture {
            get { return Parent.Fixture; }
            set { Parent.Fixture = value;  }
        }

        public override TestResult Run(EventListener listener, ITestFilter filter) {
            var result = new TestResult(this);
            switch(RunState){               
                case RunState.Runnable: testExecutor.Run(new TimedTestContext(this,
                    _ => listener.TestStarted(TestName),
                    (_, time) => {
                        result.Time = time.TotalSeconds;
                        listener.TestFinished(result);
                    }), new NUnitTestResultAdapter(result));
                    break;
                case RunState.Explicit: goto case RunState.Runnable;
            }          
            return result;
        }

        public override string TestType { get { return GetType().Name; } }

		public abstract ICustomAttributeProvider Attributes { get; }

        public virtual void Run(ITestResult testResult){}
	}
}