using Cone.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Cone.Runners
{
	public class SimpleConeRunner
	{
		class NullSuiteBuilder : IConeSuiteBuilder<ConePadSuite>
		{
			public bool SupportedType(Type type) { return true; }

			public ConePadSuite BuildSuite(Type suiteType) {
				return null;
			}
		}

		readonly IConeSuiteBuilder<ConePadSuite>[] suiteBuilders;

		public SimpleConeRunner(): this(new DefaultObjectProvider())
		{ }

		public SimpleConeRunner(ObjectProvider objectProvider) {
			suiteBuilders = new IConeSuiteBuilder<ConePadSuite>[] {
				new ConePadSuiteBuilder(objectProvider),
				new NUnitSuiteBuilder(objectProvider),
				new MSTestSuiteBuilder(objectProvider),
				new NullSuiteBuilder(),
			};
		}

		public int Workers = 1;
			
		public void RunTests(TestSession results, IEnumerable<Assembly> assemblies) {
			RunTests(results, assemblies.SelectMany(x => x.GetExportedTypes()));
		}

		public void RunTests(TestSession results, IEnumerable<Type> suiteTypes) {
			var toRun = suiteTypes
				.Choose<Type, ConePadSuite>(TryBuildSuite)
				.Flatten(x => x.Subsuites)
				.Where(x => results.IncludeSuite(x))
				.ToList();
			var claimed = -1;

			Check.Initialize();
			results.RunSession(collectResults => {
				ThreadStart runSuite = () => {
					for(;;) {
						var n = Interlocked.Increment(ref claimed);
						if(n >= toRun.Count)
							return;
						collectResults(toRun[n]);
					}
				};
				var workers = new Thread[Workers - 1];
				for (var i = 0; i != workers.Length; ++i) {
					var worker = workers[i] = new Thread(runSuite);
					worker.Start();
				}
				runSuite();
				workers.ForEach(x => x.Join());
			});
			results.Report();
		}

		bool TryBuildSuite(Type input, out ConePadSuite suite) {
			suite = suiteBuilders
				.First(x => x.SupportedType(input))
				.BuildSuite(input);
			return suite != null;
		}
	}
}
