using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace CheckThat.Expectations
{
	public interface IMethodExpectProvider
    {
        IEnumerable<MethodInfo> GetSupportedMethods();
        IExpect GetExpectation(Expression body, MethodInfo method, object target, object[] args);
    }
}
