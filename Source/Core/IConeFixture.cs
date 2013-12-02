using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface IConeFixture : ITestContext
    {
        Type FixtureType { get; }
		IEnumerable<string> Categories { get; }
		void Initialize();

		object Invoke(MethodInfo method, params object[] args);
		object GetValue(FieldInfo field);
    }
}
