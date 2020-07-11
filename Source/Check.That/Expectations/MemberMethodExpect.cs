using System.Linq.Expressions;
using System.Reflection;
using CheckThat.Internals;
using Cone.Core;

namespace CheckThat.Expectations
{
	public abstract class MethodExpect : IExpect
    {
        readonly Expression body;
        readonly MethodInfo method;
        protected readonly object[] arguments;

        protected MethodExpect(Expression body, MethodInfo method, object[] arguments) {
            this.body = body;
            this.method = method;
            this.arguments = arguments;
        }

        protected abstract object Target { get; }
        protected abstract object Actual { get ; }

        public virtual string FormatActual(IFormatter<object> formatter) =>
			formatter.Format(Actual);

        public virtual string FormatExpected(IFormatter<object> formatter) => method.Name; 

        public CheckResult Check() =>
			new CheckResult((bool)method.Invoke(Target, arguments), Maybe<object>.Some(Actual), Maybe<object>.None);

        public virtual string FormatExpression(IFormatter<Expression> formatter) =>
			formatter.Format(body);

		public virtual ConeMessage FormatMessage(IFormatter<object> formatter) =>
			ExpectMessages.EqualFormat(FormatActual(formatter), FormatExpected(formatter));
	}

    public class MemberMethodExpect : MethodExpect
    {
        readonly object target;

        public MemberMethodExpect(Expression body, MethodInfo method, object target, object[] arguments): base(body, method, arguments) {
            this.target = target;
        }

        protected override object Target { get { return target; } }
        protected override object Actual { get { return Target; } }
    }
}
