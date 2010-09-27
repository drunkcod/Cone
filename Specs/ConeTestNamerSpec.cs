using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Cone
{
    [Describe(typeof(ConeTestNamer))]
    public class ConeTestNamerSpec
    {
        ConeTestNamer TestNamer = new ConeTestNamer();

        public void converts_underscores_to_whitespace(){
            var thisMethod = MethodInfo.GetCurrentMethod();
            Verify.That(() => TestNamer.NameFor(thisMethod) == "converts underscores to whitespace");
        }

        [DisplayName("can be renamed via attribute")]
        public void Renamed() {
            var thisMethod = MethodInfo.GetCurrentMethod();
            Verify.That(() => TestNamer.NameFor(thisMethod) == "can be renamed via attribute");
        }
    }
}
