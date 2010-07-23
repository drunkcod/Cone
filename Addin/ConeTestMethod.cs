using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Core;
using System.Collections;

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

        public override string TestType { get { return GetType().Name; } }
        protected IConeTest Suite { get { return (IConeTest)Parent; } }

        public void Before() { Suite.Before(); }

        public void After(ITestResult testResult) { Suite.After(testResult); }
    }

    class ConeRowSuite : ConeTest
    {
        class ConeRowTest : ConeTest
        {
            readonly object[] parameters;

            public ConeRowTest(object[] parameters, ConeRowSuite parent, string name): base(parent, name) {
                this.parameters = parameters;
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
                    switch (RunState) {
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

            void After(ITestResult testResult) {
                Suite.After(testResult);
            }

            void Run(TestResult testResult) {
                Method.Invoke(Fixture, parameters);
                testResult.Success();
            }

            MethodInfo Method { get { return ((ConeRowSuite)Parent).method; } }
        }

        readonly MethodInfo method;
        readonly ArrayList tests;

        public ConeRowSuite(MethodInfo method, RowAttribute[] rows, Test suite, string name): base(suite, name) {
            this.method = method;
            this.tests = new ArrayList(rows.Length);
            for(int i = 0; i != rows.Length; ++i) {
                var parameters = rows[i].Parameters;
                var rowTest = new ConeRowTest(parameters, this, ConeSuite.NameFor(method, parameters));
                if (rows[i].IsPending)
                    rowTest.RunState = RunState.Ignored;
                tests.Add(rowTest); 
            }
        } 

        public override object Fixture {
            get { return Parent.Fixture; }
            set { Parent.Fixture = value; }
        }

        public override TestResult Run(EventListener listener, ITestFilter filter) {
            var testResult = new TestResult(this);
            var time = Stopwatch.StartNew();
            listener.SuiteStarted(TestName);
            try {
                foreach (Test item in Tests)
                    testResult.AddResult(item.Run(listener, filter));
            } finally {
                testResult.Time = time.Elapsed.TotalSeconds;
                listener.SuiteFinished(testResult);
            }
            return testResult;
        }

        public override bool IsSuite { get { return true; } }

        public override int TestCount { get { return tests.Count; } }

        public override IList Tests { get { return tests; } }
    }

    class ConeTestMethod : ConeTest
    {
        readonly MethodInfo method;

        public ConeTestMethod(MethodInfo method, object[] parameters, Test suite, IConeTest parent, string name) : base(suite, name) {
            this.method = method;
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
                After(new NUnitTestResultAdapter(testResult));
                listener.TestFinished(testResult);
            }
            return testResult;
        }

        void After(ITestResult testResult) {
            Suite.After(testResult);
        }

        void Run(TestResult testResult) {
            method.Invoke(Fixture, null);
            testResult.Success();
        }
    }
}
