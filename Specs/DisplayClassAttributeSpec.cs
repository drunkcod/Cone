using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Describe(typeof(DisplayClassAttribute))]
    public class DisplayClassAttributeSpec
    {
        public void default_is_value_based_ctor() {
            var displayClass = new DisplayClassAttribute(typeof(BoolDisplay));

            Verify.That(() => displayClass.DisplayFor(true).ToString() == "true");
        }

        public void with_additional_parameters() {
            var displayClass = new DisplayClassAttribute(typeof(BoolDisplay), "T", "F");

            Verify.That(() => displayClass.DisplayFor(true).ToString() == "T");
        }
    }
}
