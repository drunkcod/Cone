﻿using System;
using System.Collections.Generic;
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

		public IConeFixture Fixture => null;

		string IConeEntity.Name
		{
			get { return name; }
		}

		IEnumerable<string> IConeEntity.Categories
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

		void IConeSuite.DiscoverTests(IConeTestNamer names)
		{
			throw new NotImplementedException();
		}
	}
}
