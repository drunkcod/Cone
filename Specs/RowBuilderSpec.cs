using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Describe(typeof(RowBuilder<>))]
    public class RowBuilderSpec
    {
        void DoStuff(object arg0) { }
        int Double(int x){ return x + x; }

        public void collects_constants_parameters() {
            var rows = new RowBuilder<RowBuilderSpec>()
                .Add(x => x.DoStuff(42));
            
            Verify.That(() => rows[0].Parameters[0] == (object)42);
        }

        public void collects_computed_parameters() {
            var rows = new RowBuilder<RowBuilderSpec>()
                .Add(x => x.DoStuff(Double(21)));
            
            Verify.That(() => rows[0].Parameters[0] == (object)42);
        }

    }
}
