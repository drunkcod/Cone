using System.Collections.Generic;

namespace Cone.Core
{
	public interface IConeEntity
	{
		string Name { get; }
		IEnumerable<string> Categories { get; }
	}

	public interface IConeSuite : IConeEntity
	{
		IConeFixture Fixture { get; }
		void AddCategories(IEnumerable<string> categories);
		IRowSuite AddRowSuite(ConeMethodThunk thunk, string suiteName);
		void DiscoverTests(ConeTestNamer names);
	}
}
