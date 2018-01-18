using System.Collections.Generic;
using Cone.Runners;

namespace Cone.Core
{
	public interface IConeTestMethodSink 
	{
		void Test(Invokable method, IEnumerable<object> attributes, ConeTestMethodContext context);
		void RowTest(Invokable method, IEnumerable<IRowData> rows);
		void RowSource(Invokable method);
	}
}