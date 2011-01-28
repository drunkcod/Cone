using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone.Samples
{
    [Describe(typeof(PendingAttribute))]
    public class PendingAttributeSpec
    {
        [Pending]
        public void without_reason() { }

        [Pending(Reason = "for some reason")]
        public void for_some_reason() { }
    }
}
