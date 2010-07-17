using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Cone.Addin
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

            [Context("each with result")]
            public class AfterEachWithResult
            {
                static int Product = 1;
                [AfterAll]
                public void VerifyProduct() {
                    Verify.That(() => Product == 21);
                    Product = 1;
                }

                [AfterEach]
                public void Silly(ITestResult result) {
                   Verify.That(() => result.Status == TestStatus.Success);
                    Product *= int.Parse(result.TestName);
                }

                public void _3() { }
                public void _7() { }
            }
        }

        [Context("Nesting")]
        public class Nesting
        {
            [Context("Contexts")]
            public class Contexts
            {
                public void works_as_expected() { }
            }
        }

        [Describe(typeof(EmptySpec), Category = "Empty")]
        class EmptySpec 
        {
            [Context("with multiple categories", Category = "Empty, Context")]
            public class EmptyContext { }
        }

        public void attaches_Category_to_described_suite() {
            var suite = ConeSuite.For(typeof(EmptySpec));
            Verify.That(() => suite.Categories.Contains("Empty"));
        }

        public void multiple_categories_can_be_comma_seperated() {
            var suite = ConeSuite.For(typeof(EmptySpec)).Tests[0] as ConeSuite;
            Verify.That(() => suite.Categories.Contains("Empty"));
            Verify.That(() => suite.Categories.Contains("Context"));
        }

        public void zzz_rely_on_sorting_to_check_that_AfterAll_is_triggered() {
            //Verify.That(() => After.AfterAllExecuted == true);
            Assert.That(After.AfterAllExecuted, Is.True);
        }
    }
}
