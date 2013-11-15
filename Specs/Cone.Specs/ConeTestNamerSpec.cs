using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Core
{
    public class ValidDisplay
    {
        string value;

        public ValidDisplay(bool value) {
            this.value = value ? "Valid" : "Invalid";
        }

        public override string ToString() {
            return this.value;
        }
    }

    [Describe(typeof(ConeTestNamer))]
    public class ConeTestNamerSpec
    {
        ConeTestNamer TestNamer = new ConeTestNamer();

        public void converts_underscores_to_whitespace(){
            var thisMethod = MethodInfo.GetCurrentMethod();
            Check.That(() => TestNamer.NameFor(thisMethod) == "converts underscores to whitespace");
        }

        [DisplayAs("can be renamed via attribute")]
        public void Renamed() {
            var thisMethod = MethodInfo.GetCurrentMethod();
            Check.That(() => TestNamer.NameFor(thisMethod) == "can be renamed via attribute");
        }

        [DisplayAs("", Heading = "override heading")]
        public void WithFunkyHeading() {
            var thisMethod = MethodInfo.GetCurrentMethod();
            Check.That(() => TestNamer.NameFor(thisMethod) == "override heading");       
        }

        void MyMethod(int arg){}
        void MyMethod(object obj){}

        [Row("0x{0:x4}", 10)
        ,Row("{0,3}", 10)
        ,Row("{0,3:x2}", 10)]
        public void argument_formatting(string format, object value) {
            Expression<Action<int>> e = x => MyMethod(x);
            var myMethod = ((MethodCallExpression)(e.Body)).Method;

            Check.That(() => TestNamer.NameFor(myMethod, new object[]{ value }, new FormatString(format)) == string.Format(format, value));
        }

        [Row("{0:D}", MyEnum.Value, "0")
        ,Row("{0:D}", MyFlags.Flag1 | MyFlags.Flag2, "3")
        ,Row("flags", MyFlags.Flag1 | MyFlags.Flag2, "flags(MyFlags.Flag1 | MyFlags.Flag2)")
        ,Row("{0}", null, "")
        ,Row("not-a-format-string", null, "not-a-format-string(null)")]
        public void formatted(string format, object value, string expected) { 
            Expression<Action<object>> e = x => MyMethod(x);
            var myMethod = ((MethodCallExpression)(e.Body)).Method;

            Check.That(() => TestNamer.NameFor(myMethod, new object[]{ value }, new FormatString(format)) == expected);
        }

        void MyMethodWithDisplayClass([DisplayClass(typeof(ValidDisplay))]  bool isValid) { }

        public void obeys_DisplayClassAttribute() {
            Expression<Action<bool>> e = x => MyMethodWithDisplayClass(x);
            var target = ((MethodCallExpression)(e.Body)).Method;

            var format = new FormatString("{0}");
            Check.That(
				() => TestNamer.NameFor(target , new object[]{ true }, format) == "Valid",
				() => TestNamer.NameFor(target , new object[]{ false }, format) == "Invalid");
        }

    }
}
