using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(ConeMethodClassifier))]
    public class ConeMethodClassifierSpec
    {
        ConeMethodClass Classify<T>(Expression<Action<T>> x) {
            var method = ((MethodCallExpression)x.Body).Method;
            return new ConeMethodClassifier().Classify(method);
        }

        public void Object_methods_are_unintresting() {
            Verify.That(() => Classify<SampleFixture>(x => x.ToString()) == ConeMethodClass.Unintresting);
        }

        public void public_niladic_methods_are_tests() {
            Verify.That(() => Classify<SampleFixture>(x => x.Test()) == ConeMethodClass.Test);
        }

        public void public_with_RowAttribute_are_row_tests() {
            Verify.That(() => Classify<SampleFixture>(x => x.RowTest(42)) == ConeMethodClass.RowTest);
        }

        public void public_niladic_returning_row_test_data_sequence_is_row_source() {
            Verify.That(() => Classify<SampleFixture>(x => x.RowSource()) == ConeMethodClass.RowSource);
        }

        public void public_with_BeforeAll_are_before_all() {
            Verify.That(() => Classify<SampleFixture>(x => x.BeforeAll()) == ConeMethodClass.BeforeAll);
        }

        public void public_with_BeforeEach_are_before_each() {
            Verify.That(() => Classify<SampleFixture>(x => x.BeforeEach()) == ConeMethodClass.BeforeEach);
        }

        public void public_with_AfterEach_are_after_each() {
            Verify.That(() => Classify<SampleFixture>(x => x.AfterEach()) == ConeMethodClass.AfterEach);
        }

        public void public_with_AfterEach_and_test_result_paramaters_is_after_each_with_result() {
            Verify.That(() => Classify<SampleFixture>(x => x.AfterEachWithResult(null)) == ConeMethodClass.AfterEachWithResult);
        }

        public void public_with_AfterAll_are_after_all() {
            Verify.That(() => Classify<SampleFixture>(x => x.AfterAll()) == ConeMethodClass.AfterAll);
        }

    }
}
