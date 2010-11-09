using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone;

namespace Cone.Samples
{
    class GettingStarted {
        public int Magic;
    }

    [Describe(typeof(GettingStarted))]
    public class GettingStartedSpec
    {
        static int Mojo;
        GettingStarted Voodoo;

        [BeforeAll]
        public void InitializeTheMojo(){ Mojo = 21;}

        [BeforeEach]
        public void MagicIsTwiceTheMojo() { 
            Voodoo = new GettingStarted();
            Voodoo.Magic = Mojo + Mojo; 
        }

        [AfterEach]
        public void CleanUpTheVoodoo(){ 
            Voodoo = null;
        }

        [AfterAll]
        public void JustHereForIllustrativePurposes(){ }

        public void magic_has_been_initialized(){
            Verify.That(() => Voodoo.Magic == 42);
        }

        [Context("Verify show case")]
        public class VerifyShowCase {
            public void equal() {
                var a = 42;
                Verify.That(() => a == 42);
            }
            public void not_equal() {
                int a = 42, b = 7;
                Verify.That(() => a != b);
            }
            public void predicate() {
                var hello = "hello";
                Verify.That(() => "hello".Equals(hello));
            }
            public void inverted_predicate() {
                Verify.That(() => !"hello".Equals("world"));
            }
        }
    }
}
