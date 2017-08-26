using System.Collections.Generic;
using System.Reflection;
using Cone.Runners;

namespace Cone.Core
{
	public interface IConeTestMethodSink 
	{
		void Test(Invokable method, IEnumerable<object> attributes, ExpectedTestResult expectedResult);
		void RowTest(MethodInfo method, IEnumerable<IRowData> rows);
		void RowSource(MethodInfo method);
	}
}