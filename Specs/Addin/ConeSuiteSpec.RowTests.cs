using System.Linq;
using NUnit.Core;
using NUnit.Framework;

namespace Cone.Addin
{
    [Describe(typeof(RowTestFixture))]
    public class RowTestFixture
    {
        [Row(1, 1, 2)]
        [Row(4, 2, 42, Pending = true)]
        public void Add(int a, int b, int r) { Verify.That(() => a + b == r); }
    }

    public partial class ConeSuiteSpec
    {
        [Context("row based tests")]
        public class RowTests
        {
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
        }
    }
}
