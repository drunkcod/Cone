using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Cone
{
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
        public void Property() {
            var date = DateTime.Now;
            Verify.That(() => FormatBody(() => date.Year) == "date.Year");
        }
        public void Property_static() {
            Verify.That(() => FormatBody(() => DateTime.Now) == "DateTime.Now");
        }

        string FormatBody<T>(Expression<Func<T>> expr) {
            return new ExpressionFormatter().Format(expr.Body);
        }
    }
}
