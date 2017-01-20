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
		static readonly NullSuiteBuilder NullBuilder = new NullSuiteBuilder();

		readonly IConeSuiteBuilder<ConePadSuite>[] suiteBuilders;

		public SimpleConeRunner(ITestNamer testNamer): this(testNamer, new DefaultFixtureProvider())
		{ }

		public SimpleConeRunner(ITestNamer testNamer, FixtureProvider objectProvider) : this(
				new ConePadSuiteBuilder(testNamer, objectProvider),
				new NUnitSuiteBuilder(testNamer, objectProvider),
				new MSTestSuiteBuilder(testNamer, objectProvider)
			) { }

		SimpleConeRunner(params IConeSuiteBuilder<ConePadSuite>[] suiteBuilders) {
			this.suiteBuilders = suiteBuilders;
		}

		public static SimpleConeRunner ConeOnlyRunner(ITestNamer testNamer) {
			return new SimpleConeRunner(new ConePadSuiteBuilder(testNamer, new DefaultFixtureProvider()));
		}

		public int Workers = 1;

		public void RunTests(ICollection<string> runList, TestSession results, IEnumerable<Assembly> assemblies) {
			Check.Initialize();
			results.RunTests(CreateTestRun(
				runList, 
				BuildFlatSuites(assemblies.SelectMany(x => x.GetExportedTypes()))
				.SelectMany(x => x.Tests)));
		}

		ArraySegment<IConeTest> CreateTestRun(ICollection<string> runList, IEnumerable<IConeTest> tests) {
			var runOrder = new Dictionary<string, int>();
			var n = 0;
			foreach (var item in runList)
				try {
					runOrder.Add(item, n);
					++n;
				} catch { }
			var foundAt = new int[runList.Count + 1];
			var foundTest = new IConeTest[runList.Count + 1];
			n = 0;

			foreach(var item in tests) {
				if(runOrder.TryGetValue(item.Name, out foundAt[n])) {
					foundTest[n] = item;
					runOrder.Remove(item.Name);
					++n;
				}
			}
			Array.Sort(foundAt, foundTest, 0, n);
			return new ArraySegment<IConeTest>(foundTest, 0, n);
		}

		public void RunTests(TestSession results, IEnumerable<Assembly> assemblies) =>
			RunTests(results, assemblies.SelectMany(x => x.GetExportedTypes()));

		public void RunTests(TestSession results, IEnumerable<Type> suiteTypes) {
			Check.Initialize();
			var toRun = BuildFlatSuites(suiteTypes)
				.Where(x => results.IncludeSuite(x))
				.ToList();

			var claimed = -1;

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
		}

		IEnumerable<ConePadSuite> BuildFlatSuites(IEnumerable<Type> suiteTypes) => suiteTypes
			.Choose<Type, ConePadSuite>(TryBuildSuite)
			.Flatten(x => x.Subsuites);

		bool TryBuildSuite(Type input, out ConePadSuite suite) {
			suite = (suiteBuilders.FirstOrDefault(x => x.SupportedType(input)) ?? NullBuilder)
				.BuildSuite(input);
			return suite != null;
		}
	}
}
