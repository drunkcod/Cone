using System;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(Verify))]
    public class VerifySpec
    {
        class Counter
        {
            int next;

            public int Next() { return next++; }
        }

        public void should_evaluate_only_once() {
            var counter = new Counter();
            try {
                Verify.That(() => counter.Next() != 0);
            } catch { }
            Verify.That(() => counter.Next() == 1);
        }
        public void supports_null_values_as_actual() {
            Counter x = null;
            Verify.That(() => x == null);
        }
    }

    [Describe(typeof(Verify), "expression formatting")]
    public class VerifyFormattingSpec
    {
        public void ArrayLength() {
            var array = new int[0];
            CheckFormatting(() => array.Length == 1, Expect.New(array.Length, 1), "array.Length", "1");
        }
        public void Property() {
            var bowling = new Bowling();
            CheckFormatting(() => bowling.Score == 1, Expect.New(bowling.Score, 1), "bowling.Score", "1");
        }
        public void NotEqual() {
            var a = 42;
            try {
                Verify.That(() => a != 42);
            } catch (Exception e) {
                var message = Expect.New(a, 42).FormatNotEqual("a", "42");
                Verify.That(() => e.Message == message);
            }
        }

        void CheckFormatting(Expression<Func<bool>> expr, Expect values, string actual, string expected) {
            try {
                Verify.That(expr);
            } catch (Exception e) {
                var message = values.FormatEqual(actual, expected);
                Verify.That(() => e.Message == message);
            }
        }
    }
}
