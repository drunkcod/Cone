using System.Collections.Generic;
using System.Reflection;
using Cone.Runners;

namespace Cone.Core
{
	public interface IConeTestMethodSink 
	{
		void Test(MethodInfo method, ExpectedTestResult expectedResult);
		void RowTest(MethodInfo method, IEnumerable<IRowData> rows);
		void RowSource(MethodInfo method);
	}
}