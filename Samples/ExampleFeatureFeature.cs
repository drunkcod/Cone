
namespace Cone.Samples
{
    [Feature("ExampleFeature")]
    public class ExampleFeatureFeature
    {
        public void just_as_usual() { }

        public void moar_specs() { }

        [Context("some context")]
        public class SomeContext 
        { 
            public void same_old_same_old() { }
        }
    }
}
