using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Core;
using System.Collections;

namespace Cone.Addin
{
    abstract class ConeTest : Test
    {
        static TestName BuildTestName(Test suite, string name) {
            var testName = new TestName();
            testName.FullName = suite.TestName.FullName + "." + name;
            testName.Name = name;
            return testName;
        }

        protected ConeTest(Test suite, string name): base(BuildTestName(suite, name)) {}

        public override string TestType { get { return GetType().Name; } }
    }

    class ConeTestMethod : ConeTest
    {
        readonly MethodInfo method;
        readonly object[] parameters;
        readonly IConeTest parent;

        public ConeTestMethod(MethodInfo method, object[] parameters, Test suite, IConeTest parent, string name) : base(suite, name) {
            this.method = method;
            this.parameters= parameters;
            Parent = suite;
            this.parent = parent;
        }

        public override object Fixture {
            get { return Parent.Fixture; }
            set { Parent.Fixture = value; }
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
                After(testResult);
                listener.TestFinished(testResult);
            }
            return testResult;
        }

        void After(TestResult testResult) {
            Suite.After();
            AfterCore(testResult);
        }

        void Before() {
            Suite.Before();
        }
        void Run(TestResult testResult) {
            method.Invoke(Fixture, parameters);
            testResult.Success();
        }

        IConeTest Suite { get { return parent; } }

        protected virtual void AfterCore(TestResult testResult) { }
    }
}
