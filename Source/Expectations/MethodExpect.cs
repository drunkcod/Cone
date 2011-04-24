using System.Linq.Expressions;
using System.Reflection;

namespace Cone.Expectations
{
    public class MethodExpect : IExpect 
    {
        readonly Expression body;
        readonly MethodInfo method;
        protected readonly object target;
        protected readonly object[] arguments;

        public MethodExpect(Expression body, MethodInfo method, object target, object[] arguments) {
            this.body = body;
            this.method = method;
            this.target = target;
            this.arguments = arguments;
        }

        protected virtual object Actual { get { return target; } }
        protected virtual string FormatExpected(IFormatter<object> formatter) { 
            return method.Name; 
        }

        ExpectResult IExpect.Check() {
            return new ExpectResult {
                Actual = Actual,
                Success = (bool)method.Invoke(target, arguments)
            };
        }

        public virtual string FormatExpression(IFormatter<Expression> formatter) {
            return formatter.Format(body);
        }

        public virtual string FormatMessage(IFormatter<object> formatter) {
            return string.Format(
                ExpectMessages.EqualFormat,
                formatter.Format(Actual),
                FormatExpected(formatter));
        }
    }
}
