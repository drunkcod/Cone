using System;
using System.Linq.Expressions;
using Cone.Core;
using Cone.Expectations;
using Moq;

namespace Cone
{
    class MyValue<T>
    {
        public T Value;

        public static bool operator==(MyValue<T> left, MyValue<T> right) {
            return left.Value.Equals(right.Value);
        }

        public static bool operator!=(MyValue<T> left, MyValue<T> right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if(Object.ReferenceEquals(obj, this))
                return true;
            var other = obj as MyValue<T>;
            return other != null && other == this;
        }

        public static implicit operator T(MyValue<T> item){ return item.Value; }


        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    [Describe(typeof(EqualExpect))]
    public class BinaryExpectSpec
    {
        public void formats_actual_and_expected_values() {
            object actual = 42, expected = 7;
            var formatter = new Mock<IFormatter<object>>();
            formatter.Setup(x => x.Format(actual)).Returns("<actual>");
            formatter.Setup(x => x.Format(expected)).Returns("<expected>");

            var expect = new EqualExpect(Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(actual), Expression.Constant(expected)), new ExpectValue(actual), new ExpectValue(expected));

            var expectedMessage = string.Format(ExpectMessages.EqualFormat, "<actual>", "<expected>");
            Verify.That(() => expect.FormatMessage(formatter.Object) == expectedMessage);
        }

        public void operator_overloading_supported() {
            var actual = new MyValue<object> { Value = 42 };
            var expected = new MyValue<object> { Value = 42 };
            Expression<Func<bool>> expression = () => actual == expected;
            var expect = new BinaryExpect((BinaryExpression)expression.Body, new ExpectValue(actual), new ExpectValue(expected));

            Verify.That(() => (actual == expected) == true);
            Verify.That(() => expect.Check() == new ExpectResult { Actual = actual, Success = true });
        }
    }
}
