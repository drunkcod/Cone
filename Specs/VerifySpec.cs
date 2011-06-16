using System;

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

    [Describe(typeof(Verify))]
    public class VerifySpec
    {
        static int TheAnswer = 42;

        public void should_evaluate_only_once() {
            var counter = new Counter();
            try {
                Verify.That(() => counter.Next() != 0);
            } catch { }
            Verify.That(() => counter.Next() == 1);
        }

        public void support_constant_expressions() {
            Verify.That(() => 1 + 2 == 3);
        }

        public void support_identity_checking() {
            var obj = new Counter();
            Verify.That(() => object.ReferenceEquals(obj, obj) == true);
        }

        public void support_static_fields() {
            Verify.That(() => TheAnswer == 42);
        }

        public void negated_expression() {
            Verify.That(() => !(TheAnswer == 7));
        }

        public void type_test() {
            var objAnswer = (object)TheAnswer;
            Verify.That(() => objAnswer is Int32);
        }

        public void nullable_HasValue_when_empty() {
            Verify.That(() => new Nullable<int>().HasValue == false);
        }

        public void nullable_HasValue_with_value() {
            Verify.That(() => new Nullable<int>(42).HasValue == true);
        }

        public class PossiblyGreen
        {
            public bool IsGreen { get { return true; } }
            public static implicit operator bool(PossiblyGreen thing) { return true; }
        }

        public void bool_member_access() {
            var obj = new PossiblyGreen();
            Verify.That(() => obj.IsGreen);
        }

        public void supports_implicit_bool_conversion() {
            Verify.That(() => new PossiblyGreen());
        }

        [Context("binary expressions")]
        public class BinaryExpressions
        {
            int a = 1, b = 2;
            object obj = 2;

            public void equal() {
                var a2 = a;
                Verify.That(() => a == a2);
            }

            public void not_equal() {
                Verify.That(() => a != b);
            }

            public void less() {
                Verify.That(() => a < b);
            }

            public void less_or_equal() {
                var a2 = a;
                Verify.That(() => a <= a2);
            }

            public void greater() {
                Verify.That(() => b > a);
            }

            public void greater_or_equal() {
                Verify.That(() => b >= a);
            }

            public void and_also() {
                Verify.That(() => (a == 1) && (b == 2));
            }

            public void and_also_short_circuit_eval() {
                var rightEvaled = false;
                Func<bool> right = () => { rightEvaled = true; return true; };
                var notTrue = false;
                try {
                    Verify.That(() => notTrue && right());
                } catch { }
                Verify.That(() => rightEvaled == false);
            }

            public void return_value_is_same_as_actual() {
                Verify.That(() => Object.ReferenceEquals(Verify.That(() => obj == (object)b), obj));
            }

            class True 
            {
                public static implicit operator bool(True value){ return true; }
            }

            public void return_value_is_actual_when_using_implict_conversion() {
                var @true = new True();
                var obj = Verify.That(() => @true);
                Verify.That(() => Object.ReferenceEquals(obj, @true));
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
            }

            public void custom_equality() {
                Verify.That(() => new WithCustomEquality(42) == 42);
            }

        }

        [Context("null checks")]
        public class NullChecks
        {
            public void expected_is_null() {
                object obj = null;
                Verify.That(() => obj == null);
            }

            public void actual_and_expected_is_null() {
                Counter x = null;
                Verify.That(() => x == null);
            }
            
            public void expected_is_not_null() {
                var obj = "";
                Verify.That(() => obj != null);
            }

            public void actual_is_null_but_is_expected_not_to_be() {
                string obj = null;
                var e = Verify.Throws<Exception>.When(() => Verify.That(() => obj != null));
                Verify.That(() => e.GetType() == GetAssertionExceptionType());
            }

            public void actual_is_null_string() 
            {
                string obj = null;
                var e = Verify.Throws<Exception>.When(() => Verify.That(() => obj == ""));
                Verify.That(() => e.GetType() == GetAssertionExceptionType());
            }
            
            Type GetAssertionExceptionType() {
                try {
                    Verify.ExpectationFailed(string.Empty);
                } catch(Exception e) {
                    return e.GetType();
                }
                return null;
            }
        }

        [Context("Exceptions")]
        public class Exceptions
        {
            public void raises_expectation_failed_when_wrong_type_of_excpetion_raised() {
                try {
                    Verify.Throws<NotSupportedException>.When(() => NotImplemented());
                    throw new NotSupportedException();
                } catch (Exception e) {
                    Verify.That(() => e.GetType() == ExpectedExcpetionType());
                }
            }

            public void passes_when_Exception_types_match() {
                Verify.Throws<NotImplementedException>.When(() => NotImplemented());
            }

            class Dummy 
            {
                public int NotImplemented { get { throw new NotImplementedException(); } }
            }

            public void supports_value_expressions() {
                var obj = new Dummy();
                Verify.Throws<NotImplementedException>.When(() => obj.NotImplemented);
            }
           
            public void raises_expectation_failed_when_exception_missing() {
                try {
                    Verify.Throws<Exception>.When(() => Nothing());
                    throw new NotSupportedException();
                } catch (Exception e) {
                    Verify.That(() => e.GetType() == ExpectedExcpetionType());
                }
            }

            public void verify_Exception_message() {
                var e = Verify.Throws<NotImplementedException>.When(() => NotImplemented());
                Verify.That(() => e.GetType() == typeof(NotImplementedException));
                Verify.That(() => e.Message == new NotImplementedException().Message);
            }

            void Nothing() { }
            void NotImplemented() { throw new NotImplementedException(); }

            Type ExpectedExcpetionType() {
                try {
                    Verify.ExpectationFailed(string.Empty);
                } catch (Exception e) { 
                    return e.GetType(); 
                }
                return null;
            }
        }
    }
}
