using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Cone.Expectations
{
    public interface IMethodExpectProvider
    {
        IEnumerable<MethodInfo> GetSupportedMethods();
        IExpect GetExpectation(Expression body, MethodInfo method, object target, IEnumerable<object> args);
    }
}
