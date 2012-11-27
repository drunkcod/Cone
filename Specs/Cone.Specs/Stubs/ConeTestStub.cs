using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cone.Core;

namespace Cone.Stubs
{
	class ConeTestStub : IConeTest
	{
		string name = string.Empty;
		string context = string.Empty;
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

		public ITestName TestName
		{
			get { return new ConeTestName(context, name); }
		}

		public IConeAttributeProvider Attributes
		{
			get { throw new System.NotImplementedException(); }
		}

		public IEnumerable<string> Categories { get { return categories; } }

		public void Run(ITestResult testResult)
		{
			throw new System.NotImplementedException();
		}
	}
}
