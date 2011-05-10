using System.Linq.Expressions;
using System.Reflection;
using Cone.Core;

namespace Cone.Expectations
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

        protected virtual string FormatActual(IFormatter<object> formatter) {
            return formatter.Format(Actual);
        }

        public virtual string FormatExpected(IFormatter<object> formatter) { 
            return method.Name; 
        }

        ExpectResult IExpect.Check() {
            return new ExpectResult {
                Actual = Actual,
                Success = (bool)method.Invoke(Target, arguments)
            };
        }

        public virtual string FormatExpression(IFormatter<Expression> formatter) {
            return formatter.Format(body);
        }

        public virtual string FormatMessage(IFormatter<object> formatter) {
            return string.Format(
                ExpectMessages.EqualFormat,
                FormatActual(formatter),
                FormatExpected(formatter));
        }
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
