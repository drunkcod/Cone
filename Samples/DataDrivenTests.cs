using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Samples
{
    [Describe(typeof(DataDrivenTests))]
    public class DataDrivenTests
    {
        [Row(1, 1, 2)]
        public void Add(int a, int b, int r) { Verify.That(() => a + b == r); }
    }
}
