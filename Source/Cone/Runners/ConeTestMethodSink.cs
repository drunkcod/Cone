using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cone.Core;

namespace Cone.Runners
{
	public abstract class ConeTestMethodSink : IConeTestMethodSink
	{
		readonly ITestNamer names;
		readonly RowSuiteLookup<IRowSuite> rowSuites;

		public ConeTestMethodSink(ITestNamer names) {
			this.names = names;
			this.rowSuites = new RowSuiteLookup<IRowSuite>(CreateRowSuite);
		}

		public void Test(Invokable method, IEnumerable<object> attributes , ExpectedTestResult expectedResult) { TestCore(method, attributes, expectedResult); }

		public void RowTest(Invokable method, IEnumerable<IRowData> rows) => 
			GetRowSuite(method).Add(rows);

		public void RowSource(Invokable method) {
			var rows = ((IEnumerable<IRowTestData>)FixtureInvoke(method))
				.GroupBy(x => x.Method, x => x as IRowData);
			foreach(var item in rows)
				RowTest(new Invokable(item.Key), item);
		}

		protected abstract void TestCore(Invokable method, IEnumerable<object> attributes, ExpectedTestResult expectedResult);
		protected abstract object FixtureInvoke(Invokable method);
		protected abstract IRowSuite CreateRowSuite(ConeMethodThunk method, string context);

		protected ConeMethodThunk CreateMethodThunk(Invokable method, IEnumerable<object> attributes) {
			return new ConeMethodThunk(method, attributes, names);
		}

		IRowSuite GetRowSuite(Invokable method) =>
			rowSuites.GetSuite(CreateMethodThunk(method, method.GetCustomAttributes(true)));
	}
}