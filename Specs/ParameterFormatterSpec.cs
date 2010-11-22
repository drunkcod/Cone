using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Describe(typeof(ParameterFormatter))]
    public class ParameterFormatterSpec
    {
        ParameterFormatter Formatter = new ParameterFormatter();

        [Row(new[] { 1, 2, 3 }, "new[] { 1, 2, 3 }", DisplayAs="array elements")
        ,Row("Hello World", "\"Hello World\"", DisplayAs = "quote strings")
        ,Row(null, "null", DisplayAs= "null"), DisplayAs("special formatting")]
        public void VerifyFormat(object obj, string expected) { Verify.That(() => Formatter.Format(obj) == expected); }
    
        string FixtureProperty { get { return "42"; } }
        [Pending]
        public void fixture_property() {
            Verify.That(() => Formatter.Format(new { FixtureProperty.Length }) == "{ FixtureProperty.Length }");
        }

    }
 }
