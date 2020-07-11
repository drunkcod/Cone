using System;
using System.Collections.Generic;
using CheckThat;

namespace Cone.Core
{
    enum DummyEnum { Value }
    [Describe(typeof(ParameterFormatter))]
    public class ParameterFormatterSpec
    {
        ParameterFormatter Formatter = new ParameterFormatter();

        [Row(new [] { 1, 2, 3 }, "new [] { 1, 2, 3 }", DisplayAs="array elements")
        ,Row("Hello World", "\"Hello World\"", DisplayAs = "quote strings")
        ,Row(null, "null", DisplayAs= "null"), DisplayAs("format {0} as {1}")]
        public void VerifyFormat(object input, string expected) { Check.That(() => Formatter.Format(input) == expected); }

        public IEnumerable<IRowTestData> VerifyFormatRows() {
            return new RowBuilder<ParameterFormatterSpec>(new ConeTestNamer())
                .Add(x => x.VerifyFormat(typeof(Int32), "typeof(int)"))
                .Add(x => x.VerifyFormat(DummyEnum.Value, "DummyEnum.Value"))
                .Add(x => x.VerifyFormat(new [] { typeof(Int32) }, "new [] { typeof(int) }"));
        }
    }
 }
