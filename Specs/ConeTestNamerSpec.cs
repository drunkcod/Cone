using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

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

        [DisplayAs("can be renamed via attribute")]
        public void Renamed() {
            var thisMethod = MethodInfo.GetCurrentMethod();
            Verify.That(() => TestNamer.NameFor(thisMethod) == "can be renamed via attribute");
        }

        [Row("0x{0:x4}", 10)
        ,Row("{0,3}", 10)
        ,Row("{0,3:x2}", 10)]
        public void argument_formatting(string format, object value) {
            Expression<Action<int>> e = x => MyMethod(x);
            var myMethod = ((MethodCallExpression)(e.Body)).Method;

            Verify.That(() => TestNamer.NameFor(myMethod, new object[]{ value }, format) == string.Format(format, value));
        }

        void MyMethod(int arg){}
    }
}
