using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Samples.Failures
{
    [Feature("Failure")]
    public class FailureMessagesFeature
    {
        public void string_example() { Verify.That(() => "Hello World".Length == 3); }


        public int TheAnswer = 42;

        public void member_access_example() { Verify.That(() => TheAnswer == 7); }
    }
}
