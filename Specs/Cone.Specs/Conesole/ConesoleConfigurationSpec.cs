using System;
using System.Collections.Generic;
using Cone.Core;
using Cone.Stubs;
using Conesole;

namespace Cone.Conesole
{
	[Describe(typeof(ConesoleConfiguration))]
	public class ConesoleConfigurationSpec
	{
		public void raises_ArgumentException_for_unknown_options() {
			Verify.Throws<ArgumentException>.When(() => ConesoleConfiguration.Parse("--invalid-option"));
		}

		[Context("test filtering")]
		public class ConsoleConfigurationTestFiltering
		{
			public void ignores_context_when_only_test_name_given() {
				var includeFoo = ConesoleConfiguration.Parse("--include-tests=Foo");
				Verify.That(() => includeFoo.IncludeTest(Test().WithName("Foo")));
				Verify.That(() => !includeFoo.IncludeTest(Test().WithName("Bar")));
			}

			public void merges_multiple_includes() {
				var includeFoo = ConesoleConfiguration.Parse("--include-tests=*.Foo", "--include-tests=*.Bar");
				Verify.That(() => includeFoo.IncludeTest(Test().WithName("A.Foo")));
				Verify.That(() => includeFoo.IncludeTest(Test().WithName("A.Bar")));
			}

			public void understands_context_separator() {
				var includeFoo = ConesoleConfiguration.Parse("--include-tests=Context.Foo");
				Verify.That(() => includeFoo.IncludeTest(Test().InContext("Context").WithName("Foo")));
				Verify.That(() => !includeFoo.IncludeTest(Test().InContext("OtherContext").WithName("Foo")));
			}

			public void supports_wildcards() {
				var includeFoo = ConesoleConfiguration.Parse("--include-tests=Context.*.B*");
				Verify.That(() => includeFoo.IncludeTest(Test().InContext("Context.A").WithName("Bar")));
				Verify.That(() => includeFoo.IncludeTest(Test().InContext("Context.B").WithName("Baz")));
			}

			public void can_exclude_category() {
				var includeFoo = ConesoleConfiguration.Parse("--categories=!Acceptance");
				Verify.That(() => !includeFoo.IncludeTest(Test().WithCategories("Acceptance")));
			}

			public void can_include_category() {
				var includeWIP = ConesoleConfiguration.Parse("--categories=WIP");
				Verify.That(() => includeWIP.IncludeTest(Test().WithCategories("WIP")));
				Verify.That(() => !includeWIP.IncludeTest(Test().WithCategories("Acceptance")));
			}

			public void includes_if_any_matches() {
				var includeWIP = ConesoleConfiguration.Parse("--categories=WIP,Acceptance");
				Verify.That(() => includeWIP.IncludeTest(Test().WithCategories("WIP")));
				Verify.That(() => includeWIP.IncludeTest(Test().WithCategories("Acceptance")));
			}

			private static ConeTestStub Test() {
				return new ConeTestStub();
			}
		}
	}
}
