using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reflection;

namespace Cone.Core
{
	static class Extensions
	{
		public static bool IsOfType(this object obj, Type type) {
			return type.IsAssignableFrom(obj.GetType());
		}
	}

	public enum MyEnum { Value };

	[Flags]
	enum MyFlags { Flag1 = 1, Flag2 = 2 }
	
	[Describe(typeof(ExpressionFormatter))]
	public class ExpressionFormatterSpec
	{
		string FormatBody<T>(Expression<Func<T>> expression) { return new ExpressionFormatter(GetType()).Format(expression.Body); }

		void VerifyFormat<T>(Expression<Func<T>> expression, string result) {
			Check.That(() => FormatBody(expression) == result);
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

		public void iif() {
			var foo = false;
			VerifyFormat(() => foo ? true : false, "foo ? true : false"); 
		}

		public void coaelsce() {
			var foo = default(int?);
			VerifyFormat(() => foo ?? 0, "foo ?? 0");
		}

		public void extension_method() {
			var obj = this;
			VerifyFormat(() => obj.IsOfType(typeof(object)), "obj.IsOfType(typeof(object))");
		}

		public void equality() {
			var a = 42;
			VerifyFormat(() => a == 42, "a == 42");
		}

		public void addition() {
			int a = 1, b = 1;
			VerifyFormat(() => a + b == 2, "(a + b) == 2");
		}

		public void modulo() {
			int a = 1, b = 1;
			VerifyFormat(() => a % b == 1, "(a % b) == 1");
		}

		public void left_shift() {
			int a = 1, b = 1;
			VerifyFormat(() => a << b == 4, "(a << b) == 4");
		}

		public void right_shift() {
			int a = 1, b = 1;
			VerifyFormat(() => a >> b == 4, "(a >> b) == 4");
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
			VerifyFormat(() => new [] { date.Year }, "new [] { date.Year }");
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

		public void default_of_thing() {
			VerifyFormat(Expression.Lambda<Func<string>>(Expression.Default(typeof(string))), "default(string)");
		}

		public void inequality() {
			var a = 42;
			VerifyFormat(() => a != 42, "a != 42");
		}

		public void generic_type_instance() {
			VerifyFormat(() => typeof(Maybe<int>), "typeof(Maybe<int>)");
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

		class CheckedObject { public static implicit operator bool(CheckedObject o) => true; }
		public void cast_implicit() => VerifyFormat<bool>(() => new CheckedObject(), "new CheckedObject()");

		public void array_index() {
			var rows = new []{ 42 }; 
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

		public void enum_constant_as_actual() { 
			var expected = MyEnum.Value;
			VerifyFormat(() => MyEnum.Value == expected, "MyEnum.Value == expected"); 
		}

		public void enums_constant_as_expected() { 
			var actual = MyEnum.Value;
			VerifyFormat(() => actual == MyEnum.Value, "actual == MyEnum.Value"); 
		}

		public void flags() {
			VerifyFormat(() => MyFlags.Flag1 | MyFlags.Flag2, "MyFlags.Flag1 | MyFlags.Flag2");
		}

		public void enum_return() {
			var bar = false;
			VerifyFormat(() => GetEnum() == (bar ? MyEnum.Value : MyEnum.Value), "GetEnum() == (bar ? MyEnum.Value : MyEnum.Value)");
		}

		public void @char() {
			var a = 'a';
			VerifyFormat(() => a == 'a', "a == 'a'");
		}

		public void string_indexer() {
			VerifyFormat(() => "a"[0] == 'a', "\"a\"[0] == 'a'");
		}

		MyEnum GetEnum(){ return MyEnum.Value; }

		public void multiply() { VerifyFormat(() => A * B, "A * B"); }

		public void subtract() { VerifyFormat(() => A - B, "A - B"); }

		public void divide() { VerifyFormat(() => A / B, "A / B"); }

		public void nullable() { VerifyFormat(() => 4 == (int?)5, "4 == (int?)5"); }

		public void generic_type() { VerifyFormat(() => new List<int>(), "new List<int>()"); }

		public void invoke_niladic() { 
			Func<int> getAnswer = () => 42;
			VerifyFormat(() => getAnswer(), "getAnswer()");
		}

		public void as_expression() {
			var items = new List<object[]>{ new [] { "Hello" } };
			VerifyFormat(() => (items[0][0] as string), "(items[0][0] as string)");
		}

		[CompilerGenerated]
		class CompilerGeneratedClass 
		{
			public class Nested { }
		}

		public void compiler_generated_class_detected() {
			Check.That(
				() => ExpressionFormatter.IsCompilerGenerated(typeof(CompilerGeneratedClass)),
				() => ExpressionFormatter.IsCompilerGenerated(typeof(CompilerGeneratedClass.Nested)));
		}

		public void static_method_group_as_delegate() {
			VerifyFormat(() => Sequence.Where(MyPredicate), "Sequence.Where(MyPredicate)");
		}

		static bool MyPredicate(int n) { return false; }

		public void method_info_to_delegate() {
			var expr = Expression.Convert(
				Expression.Call(
					Expression.Constant(Check.That(() => GetType().GetMethod("MyPredicate", BindingFlags.NonPublic | BindingFlags.Static) != null), typeof(MethodInfo)), 
					typeof(MethodInfo).GetMethod("CreateDelegate", new []{ typeof(Type), typeof(object) }), 
					Expression.Constant(typeof(Func<int, bool>)), Expression.Constant(this)),
				typeof(Func<int, bool>));
			VerifyFormat(Expression.Lambda<Func<Func<int,bool>>>(expr), "MyPredicate");
		}

		IEnumerable<int> Sequence { get { yield break; } }

		[Context("nested expressions")]
		public class NestedExpressions
		{
			string FormatBody<T>(Expression<Func<T>> expression) { return new ExpressionFormatter(GetType()).Format(expression.Body); }

			void VerifyFormat<T>(Expression<Func<T>> expression, string result) {
				Check.That(() => FormatBody(expression) == result);
			}

			bool Foo(object obj) { return true; }

			class Bar 
			{
				public Bar() { }
				public Bar(object obj) { }
				public int Value;
				public int Answer;
				//to silence warnings
				public void SetValue(int value) { Value = value; }
				public void SetAnsewr(int value) { Answer = value; }
			}

			public void boolean_constant() { VerifyFormat(Expression.Lambda<Func<bool>>(Expression.Constant(true)), "true"); }

			public void function_arguments() { VerifyFormat(() => Foo(true), "Foo(true)"); }

			public void ctor_arguments() { VerifyFormat(() => new Bar(true), "new Bar(true)"); }

			public void anonymous_type() { VerifyFormat(() => new { A = 1 }, "new { A = 1 }"); }

			public void default_ctor_member_init() {
				var value = 42;
				VerifyFormat(() => new Bar { Value = value }, "new Bar { Value = value }"); 
			}

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
