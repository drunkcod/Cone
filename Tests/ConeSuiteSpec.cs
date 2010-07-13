using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Cone
{
    [Describe(typeof(ConeSuite))]
    public class ConeSuiteSpec
    {
        [Context("Before")]
        public class Before
        {
            static int Magic = 0;
            int LocalMagic;
            
            [BeforeAll]
            public void InitializeMagic() {
                Magic = 21;
            }
            [BeforeEach]
            public void DoubleLocalMagic() {
                LocalMagic = Magic * 2;
            }

            public void the_magic_is_ready() {
                Verify.That(() => Magic == 21);
                Verify.That(() => LocalMagic == 42);
            }

            public void the_magic_can_be_reused() {
                Verify.That(() => LocalMagic == 42);
            }
        }
        [Context("After")]
        public class After
        {
            internal static bool AfterAllExecuted = false;
            int Shared;

            [AfterAll]
            public void AfterAll() {
                AfterAllExecuted = true;
            }

            [AfterEach]
            public void ResetFixture() {
                Shared = 0;
            }

            public void AfterEach_called_between_tests() {
                Verify.That(() => Shared == 0);
                Shared = 1;
            }

            public void AfterEach_called_between_tests_2() {
                Verify.That(() => Shared == 0);
                Shared = 1;
            }
        }

        public void zzz_rely_on_sorting_to_check_that_AfterAll_is_triggered() {
            //Verify.That(() => After.AfterAllExecuted == true);
            Assert.That(After.AfterAllExecuted, Is.True);
        }
    }
}
