using NUnit.Core;

namespace Cone.Addin
{
    [Describe(typeof(NUnitTestNameAdapter))]
    public class NUnitTestNameAdapterSpec
    {
        TestName TestName;
        NUnitTestNameAdapter AdaptedName;

        [BeforeEach]
        public void given_a_adapted_NUnit_TestName() {
            TestName = new TestName { FullName = "Context.Hello World", Name = "Hello World" };
            AdaptedName = new NUnitTestNameAdapter(TestName);
        }

        public void Name_excludes_context() {
            Verify.That(() => AdaptedName.Name == TestName.Name);
        }

        public void Context_excludes_name() {
            Verify.That(() => !AdaptedName.Context.Contains(TestName.Name));
        }

        public void FullName_matches_full_name() {
            Verify.That(() => AdaptedName.FullName == TestName.FullName);
        }
    }
}
