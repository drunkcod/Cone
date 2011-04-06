using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(ConeMethodClassifier))]
    public class ConeMethodClassifierSpec
    {
        class Fixture
        {
            [BeforeAll]
            public void BeforeAll() { }

            [BeforeEach]
            public void BeforeEach() { }

            public void Test(){}

            [Row(42)]
            public void RowTest(int input){ }

            [AfterEach]
            public void AfterEach() { }

            [AfterEach]
            public void AfterEachWithResult(ITestResult testResult) {}

            [AfterAll]
            public void AfterAll() { }

            public IEnumerable<IRowTestData> RowSource(){ return null; }
        }

        ConeMethodClass Classify<T>(Expression<Action<T>> x) {
            var method = ((MethodCallExpression)x.Body).Method;
            return new ConeMethodClassifier().Classify(method);
        }

        public void Object_methods_are_unintresting() {
            Verify.That(() => Classify<Fixture>(x => x.ToString()) == ConeMethodClass.Unintresting);
        }

        public void public_niladic_methods_are_tests() {
            Verify.That(() => Classify<Fixture>(x => x.Test()) == ConeMethodClass.Test);
        }

        public void public_with_RowAttribute_are_row_tests() {
            Verify.That(() => Classify<Fixture>(x => x.RowTest(42)) == ConeMethodClass.RowTest);
        }

        public void public_niladic_returning_row_test_data_sequence_is_row_source() {
            Verify.That(() => Classify<Fixture>(x => x.RowSource()) == ConeMethodClass.RowSource);
        }

        public void public_with_BeforeAll_are_before_all() {
            Verify.That(() => Classify<Fixture>(x => x.BeforeAll()) == ConeMethodClass.BeforeAll);
        }

        public void public_with_BeforeEach_are_before_each() {
            Verify.That(() => Classify<Fixture>(x => x.BeforeEach()) == ConeMethodClass.BeforeEach);
        }

        public void public_with_AfterEach_are_after_each() {
            Verify.That(() => Classify<Fixture>(x => x.AfterEach()) == ConeMethodClass.AfterEach);
        }

        public void public_with_AfterEach_and_test_result_paramaters_is_after_each_with_result() {
            Verify.That(() => Classify<Fixture>(x => x.AfterEachWithResult(null)) == ConeMethodClass.AfterEachWithResult);
        }

        public void public_with_AfterAll_are_after_all() {
            Verify.That(() => Classify<Fixture>(x => x.AfterAll()) == ConeMethodClass.AfterAll);
        }

    }
}
