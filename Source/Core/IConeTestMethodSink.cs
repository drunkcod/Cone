using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
	public interface IConeTestMethodSink 
	{
		void Test(MethodInfo method);
		void RowTest(MethodInfo method, IEnumerable<IRowData> rows);
		void RowSource(MethodInfo method);
	}
}