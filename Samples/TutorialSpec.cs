using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Cone.Samples
{
    [Describe(typeof(TutorialSpec))]
    public class TutorialSpec
    {
        [Explicit]
        public void this_test_should_be_explicit() {          
            Verify.That(() => false); 
        }
    }
}
