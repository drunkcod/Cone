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

		public void RowTest(MethodInfo method, IEnumerable<IRowData> rows) {
			GetRowSuite(method).Add(rows);
		}

		public void RowSource(MethodInfo method) {
			var rows = ((IEnumerable<IRowTestData>)FixtureInvoke(method))
				.GroupBy(x => x.Method, x => x as IRowData);
			foreach(var item in rows)
				RowTest(item.Key, item);
		}

		protected abstract void TestCore(Invokable method, IEnumerable<object> attributes, ExpectedTestResult expectedResult);
		protected abstract object FixtureInvoke(MethodInfo method);
		protected abstract IRowSuite CreateRowSuite(ConeMethodThunk method, string context);

		protected ConeMethodThunk CreateMethodThunk(Invokable method, IEnumerable<object> attributes) {
			return new ConeMethodThunk(method, attributes, names);
		}

		IRowSuite GetRowSuite(MethodInfo method) {
			return rowSuites.GetSuite(CreateMethodThunk(new Invokable(method), method.GetCustomAttributes(true)));
		}
	}
}