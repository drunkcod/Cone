using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Core;

namespace Cone.Addin
{
    class ConeRowSuite : ConeTestMethod
    {
        class ConeRowTest : ConeTest
        {
            readonly object[] parameters;

            public ConeRowTest(object[] parameters, ConeRowSuite parent, string name)
                : base(parent, parent.testExecutor, name) {
                this.parameters = parameters;
            }

            public override void Run(ITestResult testResult) { Method.Invoke(Fixture, parameters); }

            MethodInfo Method { get { return ((ConeRowSuite)Parent).Method; } }
        }

        readonly List<Test> tests = new List<Test>();

        public ConeRowSuite(MethodInfo method, Test suite, TestExecutor testExecutor, string name)
            : base(method, suite, testExecutor, name) 
        { }

        public void Add(IEnumerable<IRowData> rows, ConeTestNamer testNamer) {
            foreach (var row in rows) { 
                var parameters = row.Parameters;
                var rowName = row.DisplayAs ?? testNamer.NameFor(Method, parameters);
                var rowTest = new ConeRowTest(parameters, this, rowName);
                if (row.IsPending)
                    rowTest.RunState = RunState.Ignored;
                tests.Add(rowTest);
            }
        }

        public override TestResult Run(EventListener listener, ITestFilter filter) {
            var testResult = new TestResult(this);
            var time = Stopwatch.StartNew();
            listener.SuiteStarted(TestName);
            try {
                foreach(var item in tests.Where(filter.Pass))
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
}
