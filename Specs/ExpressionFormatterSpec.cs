using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Cone
{
    static class Extensions
    {
        public static bool IsOfType(this object obj, Type type) {
            return type.IsAssignableFrom(obj.GetType());
        }
    }

    [Describe(typeof(ExpressionFormatter))]
    public class ExpressionFormatterSpec
    {
        public void ArrayLength() {
            var array = new int[0];
            Verify.That(() => FormatBody(() => array.Length) == "array.Length");
        }
        public void Call() {
            var obj = this;
            Verify.That(() => FormatBody(() => obj.GetType()) == "obj.GetType()"); 
        }
        public void Call_static() {
            Verify.That(() => FormatBody(() => DateTime.Parse("2010-07-13")) == "DateTime.Parse(\"2010-07-13\")");
        }

        public void Callt_extension_method() {
            var obj = this;
            Verify.That(() => FormatBody(() => obj.IsOfType(typeof(object))) == "obj.IsOfType(typeof(Object))");
        }

        public void Equal() {
            var a = 42;
            Verify.That(() => FormatBody(() => a == 42) == "a == 42");
        }
        public void Property() {
            var date = DateTime.Now;
            Verify.That(() => FormatBody(() => date.Year) == "date.Year");
        }
        public void Property_static() {
            Verify.That(() => FormatBody(() => DateTime.Now) == "DateTime.Now");
        }
        public void expands_quoted_expression() {
            var obj = this;
            Verify.That(() => FormatBody<Func<int>>(() => () => obj.GetHashCode()) == "() => obj.GetHashCode()");
        }
        public void Lambda() {
            Verify.That(() => FormatBody<Func<int>>(() => () => 1) == "() => 1");
        }
        public void Lambda_with_parameters() {
            Verify.That(() => FormatBody<Func<int,int,int>>(() => (x, y) => 1) == "(x, y) => 1");
        }
        public void NotEqual() {
            var a = 42;
            Verify.That(() => FormatBody(() => a != 42) == "a != 42");
        }

        string FormatBody<T>(Expression<Func<T>> expr) {
            return new ExpressionFormatter().Format(expr.Body);
        }
    }
}
