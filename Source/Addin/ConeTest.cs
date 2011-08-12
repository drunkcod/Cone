using System;
using System.Diagnostics;
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
            var testName = new TestName();
            testName.FullName = suite.TestName.FullName + "." + name;
            testName.Name = name;
            var testId = Interlocked.Increment(ref id);
            testName.TestID = new TestID(testId);
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
                case RunState.Runnable: new NUnitTestContext(this, listener, result).Run(testExecutor); break;
                case RunState.Explicit: goto case RunState.Runnable;
            }          
            return result;
        }

		class NUnitTestContext : IConeTest, ITestContext
		{
			readonly ConeTest inner;
			readonly TestResult result;
			readonly EventListener listener;
			
			public NUnitTestContext(ConeTest inner, EventListener listener, TestResult result) {
				this.inner = inner;
				this.listener = listener;
				this.result = result;
			}

            public void Run(TestExecutor runner) {
                runner.Run(this, new NUnitTestResultAdapter(result));
            }

			ICustomAttributeProvider IConeTest.Attributes { get { return inner.Attributes; } }

			void IConeTest.Run(ITestResult testResult) {
				inner.Run(testResult);
			}

			Action<ITestResult> ITestContext.Establish(IFixtureContext context, System.Action<ITestResult> next) {
				return r => {
					var time = Stopwatch.StartNew();       
                    listener.TestStarted(inner.TestName);
                    result.Timed(_ => next(r), listener.TestFinished);
				};
			}
		}

        public override string TestType { get { return GetType().Name; } }

		public abstract ICustomAttributeProvider Attributes { get; }

        public virtual void Run(ITestResult testResult){}
	}
}