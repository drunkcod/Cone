using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Feature("ExampleFeature")]
    public class ExampleFeatureFeature
    {
        public void just_as_usual() { }

        [Context("some context")]
        public class SomeContext 
        { 
            public void same_old_same_old() { }
        }
    }
}
