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
        readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        string FormatBody<T>(Expression<Func<T>> expression) { return Formatter.Format(expression.Body); }


        void VerifyFormat<T>(Expression<Func<T>> expression, string result) {
            Verify.That(() => FormatBody(expression) == result);
        }

        public void ArrayLength() {
            var array = new int[0];
            VerifyFormat(() => array.Length, "array.Length");
        }

        public void Call() {
            var obj = this;
            VerifyFormat(() => obj.GetType(), "obj.GetType()");
        }

        public void Call_static() {
            VerifyFormat(() => DateTime.Parse("2010-07-13"), "DateTime.Parse(\"2010-07-13\")");
        }

        public void Call_extension_method() {
            var obj = this;
            VerifyFormat(() => obj.IsOfType(typeof(object)), "obj.IsOfType(typeof(Object))");
        }

        public void Equal() {
            var a = 42;
            VerifyFormat(() => a == 42, "a == 42");
        }

        public void addition() {
            int a = 1, b = 1;
            VerifyFormat(() => a + b == 2, "a + b == 2");
        }

        public void Property() {
            var date = DateTime.Now;
            VerifyFormat(() => date.Year, "date.Year");
        }

        public void Property_static() {
            VerifyFormat(() => DateTime.Now, "DateTime.Now");
        }

        public void expands_quoted_expression() {
            var obj = this;
            VerifyFormat<Func<int>>(() => () => obj.GetHashCode(), "() => obj.GetHashCode()");
        }

        public void Lambda() {
            VerifyFormat<Func<int>>(() => () => 1, "() => 1");
        }

        public void Lambda_with_parameters() {
            VerifyFormat<Func<int,int,int>>(() => (x, y) => 1, "(x, y) => 1");
        }

        public void NotEqual() {
            var a = 42;
            VerifyFormat(() => a != 42, "a != 42");
        }

        public void enum_names() {
            var status = TestStatus.Failure;
            VerifyFormat(() => status == TestStatus.Success, "status == TestStatus.Success");
        }

        public void Item_indexer_properties() {
            var stuff = new Dictionary<string, int>();
            VerifyFormat(() => stuff["Answer"], "stuff[\"Answer\"]");
        }

        int A = 42, B = 7;
        public void fixture_member() {
            VerifyFormat(() => FormatBody(() => A), "FormatBody(() => A)");
        }

        public void greater() { VerifyFormat(() => A > B, "A > B"); }

        public void greater_or_equal() { VerifyFormat(() => A >= B, "A >= B"); }
        
        public void less() { VerifyFormat(() => A < B, "A < B"); }
        
        public void less_or_equal() { VerifyFormat(() => A <= B, "A <= B"); }
    }
}
