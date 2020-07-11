using System;
using System.Linq.Expressions;
using CheckThat;
using Cone.Core;
using Cone.Expectations;
using Moq;

namespace Cone
{
	[Describe(typeof(EqualExpect))]
    public class BinaryExpectSpec
    {
		class WithOverloadedOperators<T>
		{
			public T Value;

			public static bool operator==(WithOverloadedOperators<T> left, WithOverloadedOperators<T> right) {
				return left.Value.Equals(right.Value);
			}

			public static bool operator!=(WithOverloadedOperators<T> left, WithOverloadedOperators<T> right) {
				return !(left == right);
			}

			public override bool Equals(object obj) {
				if(Object.ReferenceEquals(obj, this))
					return true;
				var other = obj as WithOverloadedOperators<T>;
				return other != null && other == this;
			}

			public override int GetHashCode() {
				return Value.GetHashCode();
			}
		}

		public void formats_actual_and_expected_values() {
			object actual = 42, expected = 7;
			var formatter = new Mock<IFormatter<object>>();
			formatter.Setup(x => x.Format(actual)).Returns("<actual>");
			formatter.Setup(x => x.Format(expected)).Returns("<expected>");

			var expect = new EqualExpect(Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(actual), Expression.Constant(expected)), new ExpectValue(actual), new ExpectValue(expected));

			var expectedMessage = ExpectMessages.EqualFormat("<actual>", "<expected>").ToString();
			Check.That(() => expect.FormatMessage(formatter.Object).ToString() == expectedMessage);
		}

		public void operator_overloading_supported() {
			var actual = new WithOverloadedOperators<object> { Value = 42 };
			var expected = new WithOverloadedOperators<object> { Value = 42 };
			Expression<Func<bool>> expression = () => actual == expected;
			var expect = new BinaryExpect((BinaryExpression)expression.Body, new ExpectValue(actual), new ExpectValue(expected));

			Check.That(
				() => (actual == expected) == true,
				() => expect.Check() == new CheckResult(true, Maybe<object>.Some(actual), Maybe<object>.Some(expected)));
		}
    }
}
