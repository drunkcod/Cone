using System.Linq;
using NUnit.Core;
using NUnit.Framework;

namespace Cone.Addin
{
    [Describe(typeof(RowTestFixture))]
    public class RowTestFixture
    {
        [Row(1, 1, 2)]
        [Row(4, 2, 42, IsPending = true)]
        public void Add(int a, int b, int r) { Verify.That(() => a + b == r); }
    }

    public partial class ConeSuiteSpec
    {
        [Context("row based tests")]
        public class RowTests
        {
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
            public void create_test_per_input_row() {
                var suite = ConeSuite.For(typeof(RowTestFixture));

                Verify.That(() => suite.TestCount == 2);
            }

            public void are_named_by_their_arguments() {
                var suite = ConeSuite.For(typeof(RowTestFixture));
                var testNames = suite.Tests.Cast<ITest>().SelectMany(x => x.Tests.Cast<ITest>()).Select(x => x.TestName.Name);

                Verify.That(() => testNames.Contains("Add(1, 1, 2)"));
                Verify.That(() => testNames.Contains("Add(4, 2, 42)"));
            }

            public void zzz_put_me_last_to_check_that_AfterAll_for_rows_was_executed() {
                Verify.That(() => BeforeAndAfterRows.Magic == 0);
                Verify.That(() => BeforeAndAfterRows.Passed == 2);
                Verify.That(() => BeforeAndAfterRows.Pending == 0);//Should not execute for pending tests
            }
        }
    }
}
