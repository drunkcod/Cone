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

        public void displays_array_elements() {
            Verify.That(() => Format(new[] { 1, 2, 3 }) == "{ 1, 2, 3 }");
        }
        public void quoted_strings() {
            Verify.That(() => Format("Hello World") == "\"Hello World\"");
        }

        string Format(object obj) { return formatter.Format(obj); }
    }
}
