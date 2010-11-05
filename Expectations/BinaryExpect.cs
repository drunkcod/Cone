using System;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    public class BinaryExpect : Expect
    {
        public BinaryExpect(Expression body, object actual, object expected) : base(body, actual, expected) { }

        protected override string MessageFormat {
            get {
                if(body.NodeType == ExpressionType.NotEqual)
                    return ExpectMessages.NotEqualFormat;
                return string.Empty;
            }
        }

        protected override bool CheckCore() {
            return Expression.Lambda<Func<bool>>(
                Expression.MakeBinary(body.NodeType, Expression.Constant(actual), Expression.Constant(Expected)))
                .Execute();
        }
    }

    public class EqualExpect : Expect
    {
        public EqualExpect(Expression body, object actual, object expected): base(body, actual, expected ?? ExpectedNull.IsNull) { }

        protected override string MessageFormat { get { return ExpectMessages.EqualFormat; } }
    }
}
 