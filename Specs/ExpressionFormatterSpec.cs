using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Cone.Core
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
        string FormatBody<T>(Expression<Func<T>> expression) { return new ExpressionFormatter(GetType()).Format(expression.Body); }

        void VerifyFormat<T>(Expression<Func<T>> expression, string result) {
            Verify.That(() => FormatBody(expression) == result);
        }

        string Format<T>(Expression<Func<T>> expression) { return FormatBody(expression); }

        public void array_length() {
            var array = new int[0];
            VerifyFormat(() => array.Length, "array.Length");
        }

        public void member_call() {
            var obj = this;
            VerifyFormat(() => obj.GetType(), "obj.GetType()");
        }

        public void static_method_call() {
            VerifyFormat(() => DateTime.Parse("2010-07-13"), "DateTime.Parse(\"2010-07-13\")");
        }

        public void extension_method() {
            var obj = this;
            VerifyFormat(() => obj.IsOfType(typeof(object)), "obj.IsOfType(typeof(Object))");
        }

        public void equality() {
            var a = 42;
            VerifyFormat(() => a == 42, "a == 42");
        }

        public void addition() {
            int a = 1, b = 1;
            VerifyFormat(() => a + b == 2, "(a + b) == 2");
        }

        public void property_access() {
            var date = DateTime.Now;
            VerifyFormat(() => date.Year, "date.Year");
        }

        public void string_property_access() {
            VerifyFormat(() => "String".Length, "\"String\".Length");
        }

        public void string_method() {
            VerifyFormat(() => "String".Contains("Value"), "\"String\".Contains(\"Value\")");
        }

        public void array_with_property_access() {
            var date = DateTime.Now;
            VerifyFormat(() => new[] { date.Year }, "new[] { date.Year }");
        }

        public void type_test()
        {
            var now = (object)DateTime.Now;
            VerifyFormat(() => now is DateTime, "now is DateTime");
        }

        public void static_property_access() {
            VerifyFormat(() => DateTime.Now, "DateTime.Now");
        }

        public void expands_quoted_expression() {
            var obj = this;
            VerifyFormat<Func<int>>(() => () => obj.GetHashCode(), "() => obj.GetHashCode()");
        }

        public void lambda() {
            VerifyFormat<Func<int>>(() => () => 1, "() => 1");
        }

        public void lambda_with_singe_parameter() {
            VerifyFormat<Func<int, int>>(() => x => 1, "x => 1");
        }

        public void lambda_with_parameters() {
            VerifyFormat<Func<int,int,int>>(() => (x, y) => 1, "(x, y) => 1");
        }

        public void inequality() {
            var a = 42;
            VerifyFormat(() => a != 42, "a != 42");
        }

        public void indexer_property() {
            var stuff = new Dictionary<string, int>();
            VerifyFormat(() => stuff["Answer"], "stuff[\"Answer\"]");
        }

        public void cast_object() { VerifyFormat(() => (object)A, "A"); }

        public void cast_string() { VerifyFormat(() => (string)Obj, "(string)Obj"); }

        public void cast_Boolean() { VerifyFormat(() => (bool)Obj, "(bool)Obj"); }

        public void cast_Int32() { VerifyFormat(() => (int)Obj, "(int)Obj"); }

        public void cast_any() { VerifyFormat(() => (Expression)Obj, "(Expression)Obj"); }

        public void array_index() {
            var rows = new[]{ 42 }; 
            VerifyFormat(() => rows[0], "rows[0]");
        }

        object Obj = new object();
        int A = 42, B = 7;
        public void fixture_member() {
            VerifyFormat(() => Format(() => A), "Format(() => A)");
        }

        public void greater() { VerifyFormat(() => A > B, "A > B"); }

        public void greater_or_equal() { VerifyFormat(() => A >= B, "A >= B"); }
        
        public void less() { VerifyFormat(() => A < B, "A < B"); }
        
        public void less_or_equal() { VerifyFormat(() => A <= B, "A <= B"); }

        public void and_also() { 
            var isTrue = true;
            VerifyFormat(() => isTrue && !isTrue, "isTrue && !isTrue"); 
        }

        public void or_else() {
            var toBe = true;
            VerifyFormat(() => toBe || !toBe, "toBe || !toBe"); 
        }

        public void xor() {
            var toBe = true;
            VerifyFormat(() => toBe ^ !toBe, "toBe ^ !toBe"); 
        }

        enum MyEnum { Value };
        public void enum_constant_as_actual() { 
            var expected = MyEnum.Value;
            VerifyFormat(() => MyEnum.Value == expected, "Value == expected"); 
        }

        public void enums_constant_as_expected() { 
            var actual = MyEnum.Value;
            VerifyFormat(() => actual == MyEnum.Value, "actual == Value"); 
        }

        public void multiply() { VerifyFormat(() => A * B, "A * B"); }

        public void subtract() { VerifyFormat(() => A - B, "A - B"); }

        public void divide() { VerifyFormat(() => A / B, "A / B"); }

        public void nullable() { VerifyFormat(() => 4 == (int?)5, "4 == (int?)5"); }

        public void invoke_niladic() { 
            Func<int> getAnswer = () => 42;
            VerifyFormat(() => getAnswer(), "getAnswer()");
        }

        [Context("nested expressions")]
        public class NestedExpressions
        {
            string FormatBody<T>(Expression<Func<T>> expression) { return new ExpressionFormatter(GetType()).Format(expression.Body); }

            void VerifyFormat<T>(Expression<Func<T>> expression, string result) {
                Verify.That(() => FormatBody(expression) == result);
            }

            bool Foo(object obj) { return true; }

            class Bar 
            {
                public Bar(object obj) { }
                public int Value;
                public int Answer;
            }

            public void boolean_constant() { VerifyFormat(Expression.Lambda<Func<bool>>(Expression.Constant(true)), "true"); }

            public void function_arguments() { VerifyFormat(() => Foo(true), "Foo(true)"); }

            public void ctor_arguments() { VerifyFormat(() => new Bar(true), "new Bar(true)"); }

            public void anonymous_type() { VerifyFormat(() => new { A = 1 }, "new { A = 1 }"); }

            public void ctor_initializer() { 
                var value = 42;
                VerifyFormat(() => new Bar(null){ Value = value }, "new Bar(null){ Value = value }"); 
            }

            public void ctor_multi_initializer() { 
                var value = 42;
                VerifyFormat(() => new Bar(null){ Value = value, Answer = 42 }, "new Bar(null){ Value = value, Answer = 42 }"); 
            }

            public void parens_left() {
                int a = 1, b = 2;
                VerifyFormat(() => (a == b) == false, "(a == b) == false");
            }

            public void parens_right() {
                int a = 1, b = 2;
                VerifyFormat(() => false == (a == b), "false == (a == b)");
            }

        }

    }
}
