using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Describe(typeof(ConeSuite))]
    public class ConeSuiteSpec
    {
        object magic;
        public void creates_fixture_instance() {
            var self = this;
            Verify.That(() => self == this);
        }

        public void creates_new_fixture_per_test() {
            Verify.That(() => magic == null);
            magic = 42;
        }
        public void creates_new_fixture_per_test_2() {
            Verify.That(() => magic == null);
            magic = 7;
        }
    }
}
