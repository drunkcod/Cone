using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Core;

namespace Cone.Addin
{
    abstract class ConeTest : Test, IConeFixture
    {
        static TestName BuildTestName(Test suite, string name) {
            var testName = new TestName();
            testName.FullName = suite.TestName.FullName + "." + name;
            testName.Name = name;
            return testName;
        }

        protected ConeTest(Test suite, string name): base(BuildTestName(suite, name)) {
            Parent = suite;
        }

        public override object Fixture {
            get { return Parent.Fixture; }
            set { Parent.Fixture = value;  }
        }

        public override TestResult Run(EventListener listener, ITestFilter filter) {
            var nunitTestResult = new TestResult(this);
            ITestResult testResult = new NUnitTestResultAdapter(nunitTestResult);
            var time = Stopwatch.StartNew();
            
            listener.TestStarted(TestName);
            switch(RunState){
                case RunState.Runnable:
                    Before();
                    Guarded(() => Run(testResult), testResult.TestFailure);
                    After(testResult);
                    break;
                case RunState.Ignored: testResult.Pending("Pending"); break;
            }
            
            
            time.Stop();
            nunitTestResult.Time = time.Elapsed.TotalSeconds;
            listener.TestFinished(nunitTestResult);
            return nunitTestResult;
        }

        void Guarded(Action action, Action<Exception> handleException) {
            try {
                action();
            } catch (TargetInvocationException ex) {
                handleException(ex.InnerException);
            } catch (Exception ex) {
                handleException(ex);
            }
        }

        public override string TestType { get { return GetType().Name; } }
        protected IConeFixture Suite { get { return (IConeFixture)Parent; } }

        public void Before() { Suite.Before(); }

        public void After(ITestResult testResult) { Suite.After(testResult); }

        protected virtual void Run(ITestResult testResult){}
    }
}