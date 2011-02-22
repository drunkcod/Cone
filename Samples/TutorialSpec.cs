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

        [Context("null subexpression detection")]
        public class NullSubexpressionDetection
        {
            public void member_access_would_raise_NullReferenceException() {
                var foo = new { ThisValueIsNull = (string)null };
                Verify.That(() => foo.ThisValueIsNull.Length != 0); 
            }
            public void method_call_would_raise_NullReferenceException() {
                var foo = new { ThisValueIsNull = (string)null };
                Verify.That(() => foo.ThisValueIsNull.Contains("hello")); 
            }
        }
    }
}
