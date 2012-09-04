using System;
using Cone.Core;
using Conesole;

namespace Cone.Conesole
{
	[Describe(typeof(ConesoleConfiguration))]
	public class ConesoleConfigurationSpec
	{
		class ConeTestStub : IConeTest
		{
			string name = string.Empty;
			string context = string.Empty;

			public ConeTestStub WithName(string name) {
				this.name = name;
				return this;
			}

			public ConeTestStub InContext(string context) {
				this.context= context;
				return this;
			}

			public ITestName Name
			{
				get { return new ConeTestName(context, name); }
			}

			public IConeAttributeProvider Attributes
			{
				get { throw new System.NotImplementedException(); }
			}

			public System.Collections.Generic.IEnumerable<string> Categories
			{
				get { throw new System.NotImplementedException(); }
			}

			public void Run(ITestResult testResult)
			{
				throw new System.NotImplementedException();
			}
		}

		public void raises_ArgumentException_for_unknown_options() {
			Verify.Throws<ArgumentException>.When(() => ConesoleConfiguration.Parse("--invalid-option"));
		}

		[Context("test filtering")]
		public class ConsoleConfigurationTestFiltering
		{
			public void ignores_context_when_only_test_name_given() {
				var includeFoo = ConesoleConfiguration.Parse("--include-tests=Foo");
				Verify.That(() => includeFoo.IncludeTest(new ConeTestStub().WithName("Foo")));
				Verify.That(() => !includeFoo.IncludeTest(new ConeTestStub().WithName("Bar")));
			}

			public void understands_context_separator() {
				var includeFoo = ConesoleConfiguration.Parse("--include-tests=Context.Foo");
				Verify.That(() => includeFoo.IncludeTest(new ConeTestStub().InContext("Context").WithName("Foo")));
				Verify.That(() => !includeFoo.IncludeTest(new ConeTestStub().InContext("OtherContext").WithName("Foo")));
			}

			public void supports_wildcards() {
				var includeFoo = ConesoleConfiguration.Parse("--include-tests=Context.*.B*");
				Verify.That(() => includeFoo.IncludeTest(new ConeTestStub().InContext("Context.A").WithName("Bar")));
				Verify.That(() => includeFoo.IncludeTest(new ConeTestStub().InContext("Context.B").WithName("Baz")));
			}
		}
	}
}
