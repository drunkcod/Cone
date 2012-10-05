using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cone.Core
{
    public interface IConeSuite
    {
        string Name { get; }
		IEnumerable<string> Categories { get; }
        void AddCategories(IEnumerable<string> categories);
		IRowSuite AddRowSuite(ConeMethodThunk thunk, string suiteName);
		void DiscoverTests(ConeTestNamer names);
    }
}
