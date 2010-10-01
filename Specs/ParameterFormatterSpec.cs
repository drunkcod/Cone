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

        [Row(new[] { 1, 2, 3 }, "{ 1, 2, 3 }", Name="array elements")
        ,Row("Hello World", "\"Hello World\"", Name = "quote strings")
        ,Row(null, "null", Name= "null"), DisplayAs("special formatting")]
        public void VerifyFormat(object obj, string expected) { Verify.That(() => formatter.Format(obj) == expected); }
    }
}
