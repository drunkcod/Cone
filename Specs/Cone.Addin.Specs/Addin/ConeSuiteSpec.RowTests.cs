using System.Collections.Generic;
using System.Linq;
using NUnit.Core;
using System;

namespace Cone.Addin
{
   [Describe(typeof(RowTestFixture))]
    public class RowTestFixture
    {
        [Row(1, 1, 2)]
        [Row(4, 2, 42, IsPending = true)]
        [Row(1, 1, 2, DisplayAs = "One + One is Two")]
        public void Add(int a, int b, int r) { Verify.That(() => a + b == r); }
    }

    [Describe(typeof(DynamicRowTestFixture))]
    public class DynamicRowTestFixture
    {
        public void Add(int a, int b, int r) { Verify.That(() => a + b == r); }

        public IEnumerable<IRowTestData> Tests() {
            return new RowBuilder<DynamicRowTestFixture>()
                .Add(x => x.Add(1, 1, 2))
                .Pending(x => x.Add(4, 2, 42))
                .Add("One + One is Two", x => x.Add(1, 1, 2));
        }
    }

    [Describe(typeof(AddinSuite), "row-tests with descriptive name")]
    public class RowTestWithDescriptiveName
    {
        public void when_adding_numbers(int a, int b, int r) { }

        public IEnumerable<IRowTestData> Rows()
        {
            return new RowBuilder<RowTestWithDescriptiveName>().Add("1 + 1 == 2", x => x.when_adding_numbers(1, 1, 2));
        }
    }

    public partial class AddinSuiteSpec
    {
        [Context("row based tests")]
        public class RowTests
        {
            static readonly AddinSuiteBuilder suiteBuilder = new AddinSuiteBuilder();
            static TestSuite BuildSuite(Type type) { return suiteBuilder.BuildSuite(type); }
            public class RowTestFixtureSpec<T>
            {
                protected TestSuite Suite { get { return BuildSuite(typeof(T)); } }

                public void create_test_per_input_row() { Verify.That(() => Suite.TestCount == 3); }            

                public void rows_named_by_their_arguments() {
                    var testNames = Suite.AllTests().Select(x => x.TestName.Name);

                    Verify.That(() => testNames.Contains("Add(1, 1, 2)"));
                    Verify.That(() => testNames.Contains("Add(4, 2, 42)"));
                }

                public void can_use_custom_name() {
                    var testNames = Suite.AllTests().Select(x => x.TestName.Name);

                    Verify.That(() => testNames.Contains("One + One is Two"));                
                }
            }

            public void format_of_row_test_methods_should_equal_normal_test_methods() {
                var suite = BuildSuite(typeof(RowTestWithDescriptiveName));
                var testNames = suite.AllTests().Select(x => x.TestName.Name);
                Verify.That(() => testNames.Contains("when adding numbers"));
            }

            [Context("static row fixture")]
            public class StaticRowFixture : RowTestFixtureSpec<RowTestFixture> { }

            [Context("dynamic row fixture")]
            public class DynamicRowFixture : RowTestFixtureSpec<DynamicRowTestFixture> { }

            [Context("Before and After are triggered")]
            public class BeforeAndAfterRows
            {
                internal static int Magic;
                internal static int Passed;
                internal static int Pending;
                int Mojo;

                [BeforeAll]
                public void Initialize() {
                    Magic = 1;
                    Passed = Pending = 0;
                }

                [BeforeEach]
                public void BeforeEach() {
                    Verify.That(() => Magic == 1);
                    Mojo = Magic + Magic;
                }

                [AfterEach]
                public void AfterEach() { Mojo = 0; }

                [AfterEach]
                public void Tally(ITestResult testResult) {
                    switch (testResult.Status) {
                        case TestStatus.Success: ++Passed; break;
                        case TestStatus.Pending: ++Pending; break;
                    }
                }

                [AfterAll]
                public void AfterAll() { Magic = 0; }

                [Row(2, 0), Row(0, 2), Row(1, 1, IsPending = true)]
                public void calculate_magic(int a, int b) {
                    Verify.That(() => a + b == Mojo);
                }
            }

            public void zzz_put_me_last_to_check_that_AfterAll_for_rows_was_executed() {
                Verify.That(() => BeforeAndAfterRows.Magic == 0);
                Verify.That(() => BeforeAndAfterRows.Passed == 2);
                Verify.That(() => BeforeAndAfterRows.Pending == 0);//Should not execute for pending tests
            }
        }
    }
}
