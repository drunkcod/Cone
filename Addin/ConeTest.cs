using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Core;

namespace Cone.Addin
{
    abstract class ConeTest : Test, IConeTest
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
            var testResult = new TestResult(this);
            var time = Stopwatch.StartNew();
            try {
                listener.TestStarted(TestName);
                switch(RunState){
                    case RunState.Runnable: 
                        Before();
                        Run(testResult);
                        break;
                    case RunState.Ignored: testResult.Ignore("Pending"); break;        
                }
            } catch (TargetInvocationException e) {
                testResult.SetResult(ResultState.Failure, e.InnerException);
            } catch (Exception e) {
                testResult.SetResult(ResultState.Failure, e);
            } finally {
                testResult.Time = time.Elapsed.TotalSeconds;
                After(new NUnitTestResultAdapter(testResult));
                listener.TestFinished(testResult);
            }
            return testResult;
        }

        public override string TestType { get { return GetType().Name; } }
        protected IConeTest Suite { get { return (IConeTest)Parent; } }

        public void Before() { Suite.Before(); }

        public void After(ITestResult testResult) { Suite.After(testResult); }

        protected virtual void Run(TestResult testResult){}
    }
}