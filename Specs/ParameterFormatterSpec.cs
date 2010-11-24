using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    enum DummyEnum { Value }
    [Describe(typeof(ParameterFormatter))]
    public class ParameterFormatterSpec
    {
        ParameterFormatter Formatter = new ParameterFormatter();

        [Row(new[] { 1, 2, 3 }, "new[] { 1, 2, 3 }", DisplayAs="array elements")
        ,Row("Hello World", "\"Hello World\"", DisplayAs = "quote strings")
        ,Row(null, "null", DisplayAs= "null"), DisplayAs("format {0} as \"{1}\"")]
        public void VerifyFormat(object obj, string expected) { Verify.That(() => Formatter.Format(obj) == expected); }

        public IEnumerable<IRowTestData> VerifyFormatRows() {
            return new RowBuilder<ParameterFormatterSpec>()
                .Add(x => x.VerifyFormat(typeof(Int32), "typeof(Int32)"))
                .Add(x => x.VerifyFormat(DummyEnum.Value, "DummyEnum.Value"));
        }
    }
 }
