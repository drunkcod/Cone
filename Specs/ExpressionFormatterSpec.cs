﻿using System;
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
        static readonly ExpressionFormatter Formatter = new ExpressionFormatter();

        static string FormatBody<T>(Expression<Func<T>> expression) { return Formatter.Format(expression.Body); }

        static void VerifyFormat<T>(Expression<Func<T>> expression, string result) {
            Verify.That(() => FormatBody(expression) == result);
        }

        string Format<T>(Expression<Func<T>> expression) { return ExpressionFormatterSpec.FormatBody(expression); }


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
            VerifyFormat(() => a + b == 2, "a + b == 2");
        }

        public void property_access() {
            var date = DateTime.Now;
            VerifyFormat(() => date.Year, "date.Year");
        }

        public void array_with_property_access() {
            var date = DateTime.Now;
            VerifyFormat(() => new[] { date.Year }, "new[] { date.Year }");
        }

        public void type_test()
        {
            VerifyFormat(() => 1 is Object, "1 is object");
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

        public void enum_names() {
            var status = TestStatus.Failure;
            VerifyFormat(() => status == TestStatus.Success, "status == TestStatus.Success");
        }

        public void indexer_property() {
            var stuff = new Dictionary<string, int>();
            VerifyFormat(() => stuff["Answer"], "stuff[\"Answer\"]");
        }

        public void cast_object() { VerifyFormat(() => (object)A, "(object)A"); }

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

        [Context("nested expressions")]
        public class NestedExpressions
        {
            public void boolean_constant() {
                VerifyFormat(Expression.Lambda<Func<bool>>(Expression.Constant(true)), "true"); }
        }

    }
}
