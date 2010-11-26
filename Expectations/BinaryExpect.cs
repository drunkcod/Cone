using System;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    public class BinaryExpect : Expect
    {
        public BinaryExpect(Expression body, object actual, object expected) : base(body, actual, expected) { }

        public override string MessageFormat { get { return string.Empty; } }

        protected override bool CheckCore() {
            return Expression.Lambda<Func<bool>>(
                Expression.MakeBinary(body.NodeType, Expression.Constant(actual), Expression.Constant(Expected)))
                .Execute();
        }
    }

    public class EqualExpect : Expect
    {
        public EqualExpect(Expression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.EqualFormat; } }
    }

    public class TypeIsExpect : EqualExpect
    {
        public TypeIsExpect(Expression body, Type actual, Type expected): base(body, actual, expected) { }

        protected override bool CheckCore() {
            return ((Type)Expected).IsAssignableFrom((Type)Actual);
        }

        public override string MessageFormat { get { return ExpectMessages.EqualFormat; } }
    }

    public class NotEqualExpect : Expect 
    {
        public NotEqualExpect(Expression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.NotEqualFormat; } }

        protected override bool CheckCore() {
            return !base.CheckCore();
        }
    }

    public class LessThanExpect : BinaryExpect
    {
        public LessThanExpect(Expression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.LessThanFormat; } }
    }
       
    public class LessThanOrEqualExpect : BinaryExpect
    {
        public LessThanOrEqualExpect(Expression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.LessThanOrEqualFormat; } }
    }

    public class GreaterThanExpect : BinaryExpect
    {
        public GreaterThanExpect(Expression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.GreaterThanFormat; } }
    }

    public class GreaterThanOrEqualExpect : BinaryExpect
    {
        public GreaterThanOrEqualExpect(Expression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.GreaterThanOrEqualFormat; } }
    }

}
 