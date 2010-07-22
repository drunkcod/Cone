using System.Reflection;
using NUnit.Core;
using System;
using System.Diagnostics;

namespace Cone.Addin
{
    class ConeTestMethod : Test
    {
        static TestName BuildTestName(ConeSuite suite, string name) {
            var testName = new TestName();
            testName.FullName = suite.TestName.FullName + "." + name;
            testName.Name = name;
            return testName;
        }

        readonly MethodInfo method;
        readonly object[] parameters;

        public ConeTestMethod(MethodInfo method, object[] parameters, ConeSuite suite, string name) : base(BuildTestName(suite, name)) {
            this.method = method;
            this.parameters= parameters;
            this.Parent = suite;
        }

        public override object Fixture {  get; set; }

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
                Fixture = null;
            }
            return testResult;
        }

        void After(TestResult testResult) {
            Suite.After();
            AfterCore(testResult);
        }

        void Before() {
            Suite.Before();
            Fixture = Parent.Fixture;
        }
        void Run(TestResult testResult) {
            method.Invoke(Fixture, parameters);
            testResult.Success();
        }

        public override string TestType { get { return GetType().Name; } }

        ConeSuite Suite { get { return (ConeSuite)Parent; } }

        protected virtual void AfterCore(TestResult testResult) { }
    }
}
