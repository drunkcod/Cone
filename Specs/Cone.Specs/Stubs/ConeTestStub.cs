using System;
using System.Collections.Generic;
using Cone.Core;

namespace Cone.Stubs
{
	class ConeTestStub : IConeTest
	{
		string name = string.Empty;
		string context = string.Empty;
		string location = string.Empty;
		string[] categories = new string[0];

		public ConeTestStub WithName(string name) {
			this.name = name;
			return this;
		}

		public ConeTestStub WithCategories(params string[] categories) {
			this.categories= categories;
			return this;
		}

		public ConeTestStub InContext(string context) {
			this.context= context;
			return this;
		}

		public ConeTestStub WithLocation(string location) {
			this.location = location;
			return this;
		}

		string IConeEntity.Name => TestName.FullName;

		public string Location => location;

		public IConeFixture Fixture => null;
		public IConeSuite Suite => null;

		public ITestName TestName => new ConeTestName(context, name);

		public IConeAttributeProvider Attributes
		{
			get { throw new System.NotImplementedException(); }
		}

		public IEnumerable<string> Categories => categories;

		public void Run(ITestResult testResult)
		{
			throw new System.NotImplementedException();
		}

		public System.Reflection.Assembly Assembly
		{
			get { throw new NotImplementedException(); }
		}
	}
}
