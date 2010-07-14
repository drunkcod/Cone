using System;
using System.Linq.Expressions;
using NUnit.Framework;

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
        public void supports_null_values_as_actual() {
            Counter x = null;
            Verify.That(() => x == null);
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

        [Context("expression formatting")]
        public class ExpressionFormatting
        {
            class Bowling
            {
                public int Score { get { return 0; } }
            }

            public void Equal() {
                var bowling = new Bowling();
                CheckFormatting(() => bowling.Score == 1, Expect.Equal(bowling.Score, 1, Expect.EqualFormat), "bowling.Score", "1");
            }
            public void NotEqual() {
                var a = 42;
                CheckFormatting(() => a != 42, Expect.Equal(a, 42, Expect.NotEqualFormat), "a", "42");
            }
            public void unary_Call() {
                var foo = new Counter();
                CheckFormatting(() => foo.ReturnsFalse(), Expect.Equal(false, true, Expect.FailFormat), "foo.ReturnsFalse()", string.Empty);
            }

            void CheckFormatting(Expression<Func<bool>> expr, Expect values, string actual, string expected) {
                try {
                    Verify.That(expr);
                } catch (Exception e) {
                    var message = values.Format(actual, expected);
                    Verify.That(() => e.Message == message);
                }
            }
        }
    }
}
