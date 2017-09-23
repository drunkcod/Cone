using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Cone
{
	class Counter
	{
		int next;

		public int Next() { return next++; }
		public int Next(int step) {
			var n = next;
			next += step;
			return n;
		}

		public bool ReturnsFalse() { return false; }
	}

	interface IHasValue<T>
	{
		T Value { get; }
	}

	struct MyValue<T> : IHasValue<T>
	{
		public T Value { get; set; }

		public static implicit operator T(MyValue<T> item){ return item.Value; }
	}

	[Describe(typeof(Check))]
	public class CheckSpec
	{
		static int TheAnswer = 42;

		public void evaluates_expression_only_once() {
			var counter = new Counter();
			try {
				Check.That(() => counter.Next() != 0);
			} catch { }
			Check.That(() => counter.Next() == 1);
		}

		public void support_constant_expressions() {
			Check.That(() => 1 + 2 == 3);
		}

		public void support_identity_checking() {
			var obj = new Counter();
			Check.That(() => object.ReferenceEquals(obj, obj) == true);
		}

		public void support_static_fields() {
			Check.That(() => TheAnswer == 42);
		}

		public void negated_expression() {
			Check.That(() => !(TheAnswer == 7));
		}

		public void type_test() {
			var objAnswer = (object)TheAnswer;
			Check.That(() => objAnswer is Int32);
		}

		public void type_test_object() {
			var thing = new List<float>();
			Check.That(() => thing is ICollection<float>);
		}

		public void type_test_sequence() {
			Check.With(() => new List<double> { 3.14 }).That(xs => xs.Single() is double);

		}

		public void nullable_HasValue_when_empty() {
			Check.That(() => new Nullable<int>().HasValue == false);
		}

		public void nullable_HasValue_with_value() {
			Check.That(() => new Nullable<int>(42).HasValue == true);
		}

		public void null_coalesce() {
			var stuff = new { Value = default(int?) };
			Check.That(() => (stuff.Value ?? 0) >= 0);
		}

		public void struct_with_interface_member_access() {
			var thing = new MyValue<string> { Value = "Hello World" };
			Check.That(() => thing.Value == thing.Value);
		}

		public void value_type_return_values_are_properly_boxed() {
			var thing = new MyValue<MyValue<string>> { Value = new MyValue<string> { Value = "Hello World" } };
			Check.That(() => thing.Value.Value == thing.Value.Value);
		}

		public class PossiblyGreen
		{
			public bool IsGreen { get { return true; } }
			public static implicit operator bool(PossiblyGreen thing) { return true; }
		}

		public void bool_member_access() {
			var obj = new PossiblyGreen();
			Check.That(() => obj.IsGreen);
		}

		public void predicate() {
			Func<bool, bool> isTrue = x => x == true;
			Check.That(() => isTrue(true));
		}

		public void supports_implicit_bool_conversion() {
			Check.That(() => new PossiblyGreen());
		}

		public void aggregate_checks() {
			var e = Check<CheckFailed>.When(() =>
				Check.That(
					() => 1 == 2,
					() => 1 == 3));
			Check.That(() => e.Failures.Length == 2);
		}

		public void aggregate_collection_check() {
			var x = new List<int>();
			var e = Check<CheckFailed>.When(() =>
				Check.That(
					() => x.Count == 1,
					() => x[0] == 1));
			Check.That(() => e.Failures.Length == 2);
		}

		public void with_cast() => Check.With(() => new[] { new SqlParameter("@1", 1), new SqlParameter("@2", 1) })
			.That(x => x[0].SqlDbType == x[1].SqlDbType,x => x[0].Value == x[1].Value);

		[Context("binary expressions")]
		public class BinaryExpressions
		{
			int a = 1, b = 2;
			object obj = 2;

			public void equal() {
				var a2 = a;
				Check.That(() => a == a2);
			}

			public void not_equal() {
				Check.That(() => a != b);
			}

			public void less() {
				Check.That(() => a < b);
			}

			public void less_or_equal() {
				var a2 = a;
				Check.That(() => a <= a2);
			}

			public void greater() {
				Check.That(() => b > a);
			}

			public void greater_or_equal() {
				Check.That(() => b >= a);
			}

			public void and_also() {
				Check.That(() => (a == 1) && (b == 2));
			}

			public void and_also_short_circuit_eval() {
				var rightEvaled = false;
				Func<bool> right = () => { rightEvaled = true; return true; };
				var notTrue = false;
				try {
					Check.That(() => notTrue && right());
				} catch { }
				Check.That(() => rightEvaled == false);
			}

			public void return_value_is_same_as_actual() {
				Check.That(() => Object.ReferenceEquals(Check.That(() => obj == (object)b), obj));
			}

			class True
			{
				public static implicit operator bool(True value){ return true; }
			}

			public void return_value_is_actual_when_using_implict_conversion() {
				var @true = new True();
				var obj = Check.That(() => @true);
				Check.That(() => Object.ReferenceEquals(obj, @true));
			}

			class WithCustomEquality
			{
				readonly object value;

				public WithCustomEquality(object value){ this.value = value; }

				public override bool Equals(object obj) {
					return value.Equals(obj);
				}

				public static bool operator==(WithCustomEquality self, int value) {
					return self.Equals(value);
				}

				public static bool operator!=(WithCustomEquality self, int value) {  return !(self == value); }

				public override int GetHashCode() {
					return value.GetHashCode();
				}
			}

			public void custom_equality() {
				Check.That(() => new WithCustomEquality(42) == 42);
			}
		}

		[Context("null checks")]
		public class NullChecks
		{
			public void expected_is_null() {
				object obj = null;
				Check.That(() => obj == null);
			}

			public void actual_and_expected_is_null() {
				Counter x = null;
				Check.That(() => x == null);
			}

			public void expected_is_not_null() {
				var obj = "";
				Check.That(() => obj != null);
			}

			public void actual_is_null_but_is_expected_not_to_be() {
				string obj = null;
				var e = Check<Exception>.When(() => Check.That(() => obj != null));
				Check.That(() => e.GetType() == ExpectedExcpetionType());
			}

			public void actual_is_null_string()
			{
				string obj = null;
				var e = Check<Exception>.When(() => Check.That(() => obj == ""));
				Check.That(() => e.GetType() == ExpectedExcpetionType());
			}
		}

		[Context("Exceptions")]
		public class Exceptions
		{
			public void raises_expectation_failed_when_wrong_type_of_excpetion_raised() {
				try {
					Check<NotSupportedException>.When(() => NotImplemented());
					throw new NotSupportedException();
				} catch (Exception e) {
					Check.That(() => e.GetType() == ExpectedExcpetionType());
				}
			}

			public void passes_when_Exception_types_match() {
				Check<NotImplementedException>.When(() => NotImplemented());
			}

			class Dummy
			{
				public int NotImplemented { get { throw new NotImplementedException(); } }
			}

			public void supports_value_expressions() {
				var obj = new Dummy();
				Check<NotImplementedException>.When(() => obj.NotImplemented);
			}

			public void raises_expectation_failed_when_exception_missing() {
				try {
					Check<Exception>.When(() => Nothing());
					throw new NotSupportedException();
				} catch (Exception e) {
					Check.That(() => e.GetType() == ExpectedExcpetionType());
				}
			}

			public void verify_Exception_message() {
				var e = Check<NotImplementedException>.When(() => NotImplemented());
				Check.That(
					() => e.GetType() == typeof(NotImplementedException),
					() => e.Message == new NotImplementedException().Message);
			}

			void Nothing() { }
			void NotImplemented() { throw new NotImplementedException(); }
		}

		static Type ExpectedExcpetionType() {
			try {
				Check.That(() => 1 == 2);
			} catch(Exception e) {
				return e.GetType();
			}
			return null;
		}
    }
}