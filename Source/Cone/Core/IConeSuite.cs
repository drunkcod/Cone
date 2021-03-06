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
		IRowSuite AddRowSuite(ITestNamer names, Invokable test, string suiteName);
		void DiscoverTests(ITestNamer names);
	}
}
