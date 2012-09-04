using System;
using System.Collections.Generic;

namespace Cone.Core
{
    public interface IConeSuite
    {
        string Name { get; }
		IEnumerable<string> Categories { get; }
        void AddCategories(IEnumerable<string> categories);

		void DiscoverTests(ConeTestNamer names);
    }
}
