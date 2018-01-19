using System.Collections.Generic;
using System.Linq;
using Cone.Core;

namespace Cone.Runners
{
	public abstract class ConeTestMethodSink : IConeTestMethodSink
	{
		protected readonly ITestNamer names;
		readonly RowSuiteLookup<IRowSuite> rowSuites;

		public ConeTestMethodSink(ITestNamer names) {
			this.names = names;
			this.rowSuites = new RowSuiteLookup<IRowSuite>(names, CreateRowSuite);
		}

		public void Test(Invokable method, ConeTestMethodContext context) =>
			TestCore(method, context);

		public void RowTest(Invokable method, IEnumerable<IRowData> rows) =>
			GetRowSuite(method).Add(rows);

		public void RowSource(Invokable method) {
			var rows = ((IEnumerable<IRowTestData>)FixtureInvoke(method))
				.GroupBy(x => x.Method, x => x);
			foreach(var item in rows)
				RowTest(item.Key, item);
		}

		protected abstract void TestCore(Invokable method, ConeTestMethodContext context);
		protected abstract object FixtureInvoke(Invokable method);
		protected abstract IRowSuite CreateRowSuite(Invokable method, string context);

		IRowSuite GetRowSuite(Invokable method) =>
			rowSuites.GetSuite(method);
	}
}