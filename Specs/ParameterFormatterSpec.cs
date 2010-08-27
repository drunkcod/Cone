using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Describe(typeof(ParameterFormatter))]
    public class ParameterFormatterSpec
    {
        ParameterFormatter formatter = new ParameterFormatter();

        public void should_display_array_elements() {

            Verify.That(() => Format(new[] { 1, 2, 3 }) == "{ 1, 2, 3 }");
        }

        string Format(object obj) { return formatter.Format(obj); }
    }
}
