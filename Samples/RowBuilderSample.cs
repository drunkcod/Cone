using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Samples
{
    [Describe(typeof(RowBuilderSample))]
    public class RowBuilderSample
    {
        [Row(1, 2, 3), DisplayAs("{0} + {1} == {2}")]
        public void Addition(int a, int b, int result) {
            Check.That(() => a + b == result);
        }

        public IEnumerable<IRowTestData> AdditionExamples() {
            return new RowBuilder<RowBuilderSample>()
                .Add(x => x.Addition(1, 1, 2))
                .Add(x => x.Addition(2, 3, 5))
                .Add(x => x.Addition(2, 2, 5));
        }
    }
}
