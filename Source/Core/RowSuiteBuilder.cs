using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
	public interface IRowSuite
	{
		void Add(IEnumerable<IRowData> rows);
	}

    public class RowSuiteLookup<T> where T : IRowSuite
    {
        readonly Dictionary<string, T> items = new Dictionary<string,T>();
        readonly Func<MethodInfo, string, T> create;

        public RowSuiteLookup(Func<MethodInfo, string, T> create) {
            this.create = create;
        }

        public T GetSuite(ConeMethodThunk thunk) {
			return GetSuite(thunk.Method, thunk.GetHeading());
		}

		public T GetSuite(MethodInfo method, string name) {
            T suite;
            var key = method.Name + "." + name;
            if(!items.TryGetValue(key, out suite))
                items[key] = suite = create(method, name);
            return suite;
        }
    }
}
