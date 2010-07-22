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
        public override object Fixture { get; set; }

        public override string TestType { get { return GetType().Name; } }
    }

    class ConeRowSuite : ConeTest, IList
    {
        readonly MethodInfo method;
        readonly RowAttribute[] rows;

        public ConeRowSuite(MethodInfo method, RowAttribute[] rows, Test suite, string name): base(suite, name) {
            this.method = method;
            this.rows = rows;
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

        public override int TestCount { get { return rows.Length; } }

        public override IList Tests { get { return this; } }

        int IList.Add(object value) {
            throw new NotImplementedException();
        }

        void IList.Clear() {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value) {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value) {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value) {
            throw new NotImplementedException();
        }

        bool IList.IsFixedSize {
            get { throw new NotImplementedException(); }
        }

        bool IList.IsReadOnly {
            get { throw new NotImplementedException(); }
        }

        void IList.Remove(object value) {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index) {
            throw new NotImplementedException();
        }

        object IList.this[int index] {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        int ICollection.Count {
            get { throw new NotImplementedException(); }
        }

        bool ICollection.IsSynchronized {
            get { throw new NotImplementedException(); }
        }

        object ICollection.SyncRoot {
            get { throw new NotImplementedException(); }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            foreach (var item in rows) {
                var test = new ConeTestMethod(method, item.Parameters, (ConeSuite)Parent, (ConeSuite)Parent, ConeSuite.NameFor(method, item.Parameters).ToUpperInvariant());
                if (item.IsPending)
                    test.RunState = RunState.Ignored;
                yield return test;
            }
        }
    }

    class ConeTestMethod : ConeTest
    {
        readonly MethodInfo method;
        readonly object[] parameters;
        readonly IConeTest parent;

        public ConeTestMethod(MethodInfo method, object[] parameters, Test suite, IConeTest parent, string name) : base(suite, name) {
            this.method = method;
            this.parameters= parameters;
            this.Parent = suite;
            this.parent = parent;
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
            Fixture = null;
        }

        void Before() {
            Suite.Before();
            Fixture = Parent.Fixture;
        }
        void Run(TestResult testResult) {
            method.Invoke(Fixture, parameters);
            testResult.Success();
        }

        IConeTest Suite { get { return parent; } }

        protected virtual void AfterCore(TestResult testResult) { }
    }
}
