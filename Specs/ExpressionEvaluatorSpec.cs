using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Describe(typeof(ExpressionEvaluator))]
    public class ExpressionEvaluatorSpec
    {
        public void constant_evaluation() {
            Verify.That(() => ExpressionEvaluator.Evaluate(() => 42) == 42);
        }
    }
}
