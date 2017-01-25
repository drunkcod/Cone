using System;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
{
    public class BinaryExpect : Expect
    {
        readonly MethodInfo method;

        public BinaryExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected) : base(body, actual, expected) {            
            this.method = body.Method;
        }

        public override ConeMessage MessageFormat(string actual, string expected) => ConeMessage.Empty;

        protected override bool CheckCore() {
            if(method != null && method.IsStatic)
                return (bool)method.Invoke(null, new []{ ActualValue, ExpectedValue });

            return Expression.Lambda<Func<bool>>(Expression.MakeBinary(body.NodeType, 
                Expression.Constant(ActualValue), 
                Expression.Constant(ExpectedValue))).Execute();
        }
    }

    public class EqualExpect : Expect
    {
        public EqualExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): base(body, actual, expected) 
		{ }
    }

    public class TypeIsExpect : Expect
    {
        public TypeIsExpect(Expression body, IExpectValue actual, Type expected): base(body, actual, new ExpectValue(expected)) { }

        Type ActualType { get { return ActualValue == null ? null : ActualValue.GetType(); } }

        protected override bool CheckCore() {
            return ((Type)ExpectedValue).IsAssignableFrom(ActualType);
        }
    }

    public class NotEqualExpect : Expect 
    {
        public NotEqualExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): base(body, actual, expected) { }

        public override ConeMessage MessageFormat(string actual, string expected) => ConeMessage.Format(ExpectMessages.NotEqualFormat, actual, expected);

        protected override bool CheckCore() {
            return !base.CheckCore();
        }
    }

    public class LessThanExpect : BinaryExpect
    {
        public LessThanExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): base(body, actual, expected) { }

        public override ConeMessage MessageFormat(string actual, string expected) => ConeMessage.Format(ExpectMessages.LessThanFormat, actual, expected);
    }
       
    public class LessThanOrEqualExpect : BinaryExpect
    {
        public LessThanOrEqualExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): base(body, actual, expected) { }

        public override ConeMessage MessageFormat(string actual, string expected) => ConeMessage.Format(ExpectMessages.LessThanOrEqualFormat, actual, expected);
    }

    public class GreaterThanExpect : BinaryExpect
    {
        public GreaterThanExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): base(body, actual, expected) { }

        public override ConeMessage MessageFormat(string actual, string expected) => ConeMessage.Format(ExpectMessages.GreaterThanFormat, actual, expected);
    }

    public class GreaterThanOrEqualExpect : BinaryExpect
    {
        public GreaterThanOrEqualExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): base(body, actual, expected) { }

        public override ConeMessage MessageFormat(string actual, string expected) => ConeMessage.Format(ExpectMessages.GreaterThanOrEqualFormat, actual, expected);
    }
}
 