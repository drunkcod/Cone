﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
{
    public class BinaryExpect : Expect
    {
        readonly MethodInfo method;

        public BinaryExpect(BinaryExpression body, object actual, object expected) : base(body, actual, expected) {            
            this.method = body.Method;
        }

        public override string MessageFormat { get { return string.Empty; } }

        protected override bool CheckCore() {
            if(method != null && method.IsStatic)
                return (bool)method.Invoke(null, new[]{ ActualValue, ExpectedValue });

            return Expression.Lambda<Func<bool>>(Expression.MakeBinary(body.NodeType, 
                Expression.Constant(ActualValue), 
                Expression.Constant(ExpectedValue))).Execute();
        }
    }

    public class EqualExpect : Expect
    {
        public EqualExpect(BinaryExpression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.EqualFormat; } }
    }

    public class TypeIsExpect : Expect
    {
        public TypeIsExpect(Expression body, object actual, Type expected): base(body, actual, expected) { }

        Type ActualType { get { return ActualValue == null ? null : ActualValue.GetType(); } }

        protected override bool CheckCore() {
            return ((Type)ExpectedValue).IsAssignableFrom(ActualType);
        }

        public override string MessageFormat { get { return ExpectMessages.EqualFormat; } }
    }

    public class NotEqualExpect : Expect 
    {
        public NotEqualExpect(BinaryExpression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.NotEqualFormat; } }

        protected override bool CheckCore() {
            return !base.CheckCore();
        }
    }

    public class LessThanExpect : BinaryExpect
    {
        public LessThanExpect(BinaryExpression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.LessThanFormat; } }
    }
       
    public class LessThanOrEqualExpect : BinaryExpect
    {
        public LessThanOrEqualExpect(BinaryExpression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.LessThanOrEqualFormat; } }
    }

    public class GreaterThanExpect : BinaryExpect
    {
        public GreaterThanExpect(BinaryExpression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.GreaterThanFormat; } }
    }

    public class GreaterThanOrEqualExpect : BinaryExpect
    {
        public GreaterThanOrEqualExpect(BinaryExpression body, object actual, object expected): base(body, actual, expected) { }

        public override string MessageFormat { get { return ExpectMessages.GreaterThanOrEqualFormat; } }
    }

}
 