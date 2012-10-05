using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	public abstract class ConeTestMethodSink : IConeTestMethodSink
	{
		readonly ConeTestNamer names;
		readonly RowSuiteLookup<IRowSuite> rowSuites;


		public ConeTestMethodSink(ConeTestNamer names) {
			this.names = names;
			this.rowSuites = new RowSuiteLookup<IRowSuite>(CreateRowSuite);
		}

		public void Test(MethodInfo method) { TestCore(method); }

		public void RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
			GetRowSuite(method).Add(rows);
		}

		public void RowSource(MethodInfo method) {                 
			var rows = ((IEnumerable<IRowTestData>)FixtureInvoke(method))
				.GroupBy(x => x.Method, x => x as IRowData);
			foreach(var item in rows)
				RowTest(item.Key, item);
		}

		protected abstract void TestCore(MethodInfo method);
		protected abstract object FixtureInvoke(MethodInfo method);
		protected abstract IRowSuite CreateRowSuite(MethodInfo method, string context);

		protected ConeMethodThunk CreateMethodThunk(MethodInfo method) {
			return new ConeMethodThunk(method, names);
		}

		IRowSuite GetRowSuite(MethodInfo method) {
			return rowSuites.GetSuite(CreateMethodThunk(method));
		}
	}
}