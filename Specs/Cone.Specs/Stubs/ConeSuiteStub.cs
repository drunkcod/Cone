using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone.Core;

namespace Cone.Stubs
{
	class ConeSuiteStub : IConeSuite
	{
		string name;

		public ConeSuiteStub WithName(string name) {
			this.name = name;
			return this;
		}

		string IConeSuite.Name
		{
			get { return name; }
		}

		IEnumerable<string> IConeSuite.Categories
		{
			get { throw new NotImplementedException(); }
		}

		void IConeSuite.AddCategories(IEnumerable<string> categories)
		{
			throw new NotImplementedException();
		}

		IRowSuite IConeSuite.AddRowSuite(ConeMethodThunk thunk, string suiteName)
		{
			throw new NotImplementedException();
		}

		void IConeSuite.DiscoverTests(ConeTestNamer names)
		{
			throw new NotImplementedException();
		}
	}
}
