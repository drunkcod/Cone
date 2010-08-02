using System.Collections;
using System.Diagnostics;
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
                : base(parent, name) {
                this.parameters = parameters;
            }

            protected override void Run(TestResult testResult) {
                Method.Invoke(Fixture, parameters);
                testResult.Success();
            }

            MethodInfo Method { get { return ((ConeRowSuite)Parent).Method; } }
        }

        readonly ArrayList tests;

        public ConeRowSuite(MethodInfo method, RowAttribute[] rows, Test suite, string name)
            : base(method, suite, name) {
            this.tests = new ArrayList(rows.Length);
            for (int i = 0; i != rows.Length; ++i) {
                var parameters = rows[i].Parameters;
                var rowTest = new ConeRowTest(parameters, this, ConeTestNamer.NameFor(method, parameters));
                if (rows[i].IsPending)
                    rowTest.RunState = RunState.Ignored;
                tests.Add(rowTest);
            }
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
}
