using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [Describe(typeof(ExpressionEvaluator))]
    public class ExpressionEvaluatorSpec
    {
        public void constant_evaluation() {
            Verify.That(() => ExpressionEvaluator.Evaluate(() => 42) == 42);
        }

        public void subexpression_null_check_not_equal() {
            var foo = new { ThisValueIsNull = (string)null };
            Verify.Throws<NullSubexpressionException>.When(() => foo.ThisValueIsNull.Length != 0);
        }

        public void subexpression_null_check_equal() {
            var foo = new { ThisValueIsNull = (string)null };
            Verify.Throws<NullSubexpressionException>.When(() => foo.ThisValueIsNull.Length == 0);
        }

        public void subexpression_null_check_method_call() {
            var foo = new { ThisValueIsNull = (string)null };
            Verify.Throws<NullSubexpressionException>.When(() => foo.ThisValueIsNull.Contains("foo") == true);
        }

        public void null_check_unary_expression() {
            var foo = new { ThisValueIsNull = (string)null };
            Verify.Throws<NullSubexpressionException>.When(() => foo.ThisValueIsNull.Contains("foo"));
        }

        public void new_expression_propagates_correct_exception() {
            Verify.Throws<ArgumentNullException>.When(() => new List<object>(null));
        }

        class MyValue<T> 
        {
            public T Value;

            public static implicit operator T(MyValue<T> item){ return item.Value; }
        }

        public void implicit_convesion_operators() {
            Verify.That(() => new MyValue<int>{ Value = 42 } == 42);
        }

        struct MyValueObject { }

        public void construct_value_object() {
            Verify.That(() => new MyValueObject().Equals(new MyValueObject()));
        }

        public void out_parameters() {
            var item = new object();
            var stuff = new Dictionary<string, object> { { "Key", item } };
            object value = null;
            Verify.That(() => stuff.TryGetValue("Key", out value));
            Verify.That(() => value == item);
        }

        public void subexpression_null_check_provides_proper_supexpression() {
            var foo = new { ThisValueIsNull = (string)null };
            var error = Verify.Throws<NullSubexpressionException>.When(() => foo.ThisValueIsNull.Length == 0);
            var formatter = new ExpressionFormatter(GetType());
            Verify.That(() => formatter.Format(error.NullSubexpression) == "foo.ThisValueIsNull");
            Verify.That(() => formatter.Format(error.Expression) == "foo.ThisValueIsNull.Length == 0");
        }
    }
}
