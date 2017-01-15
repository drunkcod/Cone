using System;

namespace Cone.Core
{
	[Serializable]
	public struct ConeTestName : ITestName
	{
		public ConeTestName(string context, string name) {
			this.Context = context;
			this.Name = name.TrimStart();
		}

		public static ConeTestName From(ITestName testName)=>
			new ConeTestName(testName.Context, testName.Name);

		public string Context { get; }
		public string Name { get; }

		public string FullName => $"{Context}.{Name}";
	}
}