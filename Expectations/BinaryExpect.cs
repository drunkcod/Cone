using System;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    class ExpectNull
    {
        public override bool Equals(object obj) {
            return obj == null;
        }

        public override int GetHashCode() {
            return 0;
        }

        public override string ToString() {
            return "null";
        }
    }

    public class BinaryExpect : Expect
    {
        static readonly ExpectNull ExpectNull = new ExpectNull();

        public BinaryExpect(Expression body, object actual, object expected)
            : base(body, actual, expected ?? ExpectNull) {
        }

        public override string FormatMessage(IFormatter<object> formatter) {
            return string.Format(MessageFormat, formatter.Format(actual), formatter.Format(expected));
        }

        protected virtual string MessageFormat {
            get {
                if(body.NodeType == ExpressionType.NotEqual)
                    return ExpectMessages.NotEqualFormat;
                return string.Empty;
            }
        }

        protected override bool CheckCore() {
            return Expression.Lambda<Func<bool>>(
                Expression.MakeBinary(body.NodeType, Expression.Constant(actual), Expression.Constant(expected)))
                .Execute();
        }
    }

    public class BinaryEqualExpect : BinaryExpect
    {
        public BinaryEqualExpect(Expression body, object actual, object expected): base(body, actual, expected) { }

        protected override string MessageFormat { get { return ExpectMessages.EqualFormat; } }        

        protected override bool CheckCore() {
            return expected.Equals(actual);
        }
    }
}
 