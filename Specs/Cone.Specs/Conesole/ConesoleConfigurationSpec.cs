using System;
using CheckThat;
using Cone.Stubs;
using Cone.Worker;

namespace Cone.Conesole
{
	[Describe(typeof(WorkerConfiguration))]
	public class WorkerConfigurationSpec
	{
		public void raises_ArgumentException_for_unknown_options() {
			Check<ArgumentException>.When(() => WorkerConfiguration.Parse("--invalid-option"));
		}

		[Context("test filtering")]
		public class ConsoleConfigurationTestFiltering
		{
			public void ignores_context_when_only_test_name_given() {
				var includeFoo = WorkerConfiguration.Parse("--include-tests=Foo");
				Check.That(() => includeFoo.IncludeTest(Test().WithName("Foo")));
				Check.That(() => !includeFoo.IncludeTest(Test().WithName("Bar")));
			}

			public void merges_multiple_includes() {
				var includeFoo = WorkerConfiguration.Parse("--include-tests=*.Foo", "--include-tests=*.Bar");
				Check.That(() => includeFoo.IncludeTest(Test().WithName("A.Foo")));
				Check.That(() => includeFoo.IncludeTest(Test().WithName("A.Bar")));
			}

			public void understands_context_separator() {
				var includeFoo = WorkerConfiguration.Parse("--include-tests=Context.Foo");
				Check.That(() => includeFoo.IncludeTest(Test().InContext("Context").WithName("Foo")));
				Check.That(() => !includeFoo.IncludeTest(Test().InContext("OtherContext").WithName("Foo")));
			}

			public void supports_wildcards() {
				var includeFoo = WorkerConfiguration.Parse("--include-tests=Context.*.B*");
				Check.That(() => includeFoo.IncludeTest(Test().InContext("Context.A").WithName("Bar")));
				Check.That(() => includeFoo.IncludeTest(Test().InContext("Context.B").WithName("Baz")));
			}

			public void can_exclude_category() {
				var includeFoo = WorkerConfiguration.Parse("--categories=!Acceptance");
				Check.That(() => !includeFoo.IncludeTest(Test().WithCategories("Acceptance")));
			}

			public void can_include_category() {
				var includeWIP = WorkerConfiguration.Parse("--categories=WIP");
				Check.That(() => includeWIP.IncludeTest(Test().WithCategories("WIP")));
				Check.That(() => !includeWIP.IncludeTest(Test().WithCategories("Acceptance")));
			}

			public void includes_if_any_matches() {
				var includeWIP = WorkerConfiguration.Parse("--categories=WIP,Acceptance");
				Check.That(() => includeWIP.IncludeTest(Test().WithCategories("WIP")));
				Check.That(() => includeWIP.IncludeTest(Test().WithCategories("Acceptance")));
			}

			public void supports_combining_test_pattern_and_categories() {
				var pattern = WorkerConfiguration.Parse("--categories=WIP", "--include-tests=*.Foo");
				Check.That(() => pattern.IncludeTest(Test().WithName("A.Foo")) == false);
				Check.That(() => pattern.IncludeTest(Test().WithCategories("WIP").WithName("A.Bar")) == false);
			}

			private static ConeTestStub Test() {
				return new ConeTestStub();
			}
		}
	}
}
