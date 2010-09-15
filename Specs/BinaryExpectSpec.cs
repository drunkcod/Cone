using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using System.Linq.Expressions;

namespace Cone
{
    [Describe(typeof(BinaryExpect))]
    public class BinaryExpectSpec
    {
        public void formats_actual_and_expected_values() {
            object actual = 42, expected = 7;
            var formatter = new Mock<IFormatter<object>>();
            formatter.Setup(x => x.Format(actual)).Returns("<actual>");
            formatter.Setup(x => x.Format(expected)).Returns("<expected>");

            var expect = new BinaryExpect(Expression.MakeBinary(ExpressionType.Equal, Expression.Constant(actual), Expression.Constant(expected)), actual, expected);

            var expectedMessage = string.Format(BinaryExpect.EqualFormat, "<actual>", "<expected>");
            Verify.That(() => expect.FormatMessage(formatter.Object) == expectedMessage);
        }
    }
}
