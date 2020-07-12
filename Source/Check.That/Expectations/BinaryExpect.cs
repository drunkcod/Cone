using System;
using System.Linq.Expressions;
using System.Reflection;
using CheckThat.Internals;
using Cone.Core;

namespace CheckThat.Expectations
{
    public class BinaryExpect : Expect
    {
        readonly MethodInfo method;

        public BinaryExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected) : base(body, actual, expected) {            
            this.method = body.Method;
        }

        protected override bool CheckCore() {
			var e = Expected == ExpectValue.Null ? null : Expected.Value;
			if(method != null && method.IsStatic)
                return (bool)method.Invoke(null, new []{ ActualValue, e});
            return CheckOp(ActualValue, e);
        }

		protected virtual bool CheckOp(object actual, object expected) =>
			Expression.Lambda<Func<bool>>(Expression.MakeBinary(body.NodeType, 
                Expression.Constant(actual), 
                Expression.Constant(expected))).Execute();
    }

    public class EqualExpect : BinaryExpect
    {
        public EqualExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): 
			base(body, actual, expected) 
		{ }

		protected override bool CheckOp(object actual, object expected) => Equals(actual, expected);
	}

    public class NotEqualExpect : BinaryExpect 
    {
        public NotEqualExpect(BinaryExpression body, IExpectValue actual, IExpectValue expected): base(body, actual, expected) { }

        public override ConeMessage MessageFormat(string actual, string expected) => ConeMessage.Format(ExpectMessages.NotEqualFormat, actual, expected);

		protected override bool CheckOp(object actual, object expected) => !Equals(actual, expected);
    }

    public class TypeIsExpect : Expect
    {
		readonly Type expectedType;
		readonly Type actualType;

        public TypeIsExpect(Expression body, Type actualType, IExpectValue actual, Type expected): base(body, actual, new ExpectValue(expected)) { 
			this.expectedType = expected;
			this.actualType = actual.Value == null ? actualType : actual.Value.GetType();	
		}

		public override string FormatActual(IFormatter<object> formatter) =>
			formatter.Format(actualType);

		protected override bool CheckCore() => 
			expectedType.IsAssignableFrom(actualType);
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
 