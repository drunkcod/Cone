using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Cone.Core;

namespace Cone
{
    [Describe(typeof(ExpressionEvaluator))]
    public class ExpressionEvaluatorSpec
    {
        ExpressionEvaluator evaluator = new ExpressionEvaluator {
            Unsupported = x => { throw new NotSupportedException("Unsupported expression type:" + x.NodeType.ToString()); }
        };

        T Evaluate<T>(Expression<Func<T>> lambda){ return Evaluate(lambda, x => { throw x.Exception; }); }
        T Evaluate<T>(Expression<Func<T>> lambda, Func<EvaluationResult, EvaluationResult> onError){ return (T)evaluator.Evaluate(lambda.Body, lambda, onError).Result; }
        void EvaluateError<T>(Expression<Func<T>> lambda, Func<EvaluationResult, EvaluationResult> onError) { 
            var result = evaluator.Evaluate(lambda.Body, lambda, onError);
            Verify.That(() => result.IsError);
        }

        public void constant_evaluation() {
            Verify.That(() => Evaluate(() => 42) == 42);
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

        public void not_equals() {
            int a = 2, b = 1;
            Expression<Func<bool>> binaryOp = () => (a != b) == true;
            Verify.That(() => Evaluate(binaryOp) == true);
        }

        public void value_type_promotion() {
            var a = new byte[]{ 1 };
            var b = a;
            Verify.That(() => Evaluate(() => a[0] == b[0]));
        }

        public void null_equality() {
            Verify.That(() => Evaluate(() => (DateTime?)null) == null);
        }

        class MyValue<T> 
        {
            public T Value;

            public static implicit operator T(MyValue<T> item){ return item.Value; }
        }

        public void implicit_convesion_operators() {
            Verify.That(() => new MyValue<int>{ Value = 42 } == 42);
        }

        public void invoke_niladic() {
            Func<int> getAnswer = () => 42;

            Verify.That(() => Evaluate(() => getAnswer()) == 42);
        }

        public void invoke_target_raises_exception() {
            Func<int> getAnswer = () => { throw new NotImplementedException(); };

            Verify.Throws<NotImplementedException>.When(() => Evaluate(() => getAnswer()));
        }

        public void lambda_parameters() {
            Func<Func<int>, int> addOne = f => f() + 1;

            Verify.That(() => Evaluate(() => addOne(() => 1)) == 2);
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
            Verify.That(() => formatter.Format(error.Expression) == "foo.ThisValueIsNull");
            Verify.That(() => formatter.Format(error.Context) == "foo.ThisValueIsNull.Length == 0");
        }

        public void detect_errors_during_member_access() {
            var formatter = new ExpressionFormatter(GetType());

            EvaluateError(() => Throws<string>().Length, x => {
                Verify.That(() => formatter.Format(x.Expression) == "Throws()");
                return x; 
            });
        }

        public void detect_errors_when_computing_arguments() {
            var formatter = new ExpressionFormatter(GetType());
            EvaluateError(() => Object.Equals(Throws<string>(), ""), x => {
                Verify.That(() => formatter.Format(x.Expression) == "Throws()");
                return x; 
            });
        }

        class MyDsl
        {
            public MyDsl DoStuff() { return this; }
            public MyDsl NotImplemented() { throw new NotImplementedException(); }
            public static implicit operator bool(MyDsl value){ return true; }
        }

        public void detect_errors_during_conversion() {
            var formatter = new ExpressionFormatter(GetType());
            var dsl = new MyDsl();
            EvaluateError(() => (bool)(dsl.DoStuff().NotImplemented().DoStuff()), x => x);
        }

        T Throws<T>() { throw new NotImplementedException(); }
    }
}
