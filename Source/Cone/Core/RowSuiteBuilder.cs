using System;
using System.Collections.Generic;

namespace Cone.Core
{
	public interface IRowSuite
	{
		void Add(IEnumerable<IRowData> rows);
	}

	public class RowSuiteLookup<T> where T : IRowSuite
	{
		readonly Dictionary<string, T> items = new Dictionary<string,T>();
		readonly ITestNamer names;
		readonly Func<Invokable, string, T> create;

		public RowSuiteLookup(ITestNamer names, Func<Invokable, string, T> create) {
			this.names = names;
			this.create = create;
		}

		public T GetSuite(Invokable test) {
			return GetSuite(test, names.NameFor(test));
		}

		public T GetSuite(Invokable method, string name) {
			T suite;
			var key = method.Name + "." + name;
			if (!items.TryGetValue(key, out suite))
				items[key] = suite = create(method, name);
			return suite;
		}
    }
}
