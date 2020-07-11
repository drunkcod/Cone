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
		class NullSuiteBuilder : IConeSuiteBuilder<ConeSuite>
		{
			public bool SupportedType(Type type) { return true; }

			public ConeSuite BuildSuite(Type suiteType) {
				return null;
			}
		}
		static readonly NullSuiteBuilder NullBuilder = new NullSuiteBuilder();

		readonly IConeSuiteBuilder<ConeSuite>[] suiteBuilders;

		public SimpleConeRunner(ITestNamer testNamer): this(testNamer, new DefaultFixtureProvider())
		{ }

		public SimpleConeRunner(ITestNamer testNamer, FixtureProvider objectProvider) : this(
				new ConePadSuiteBuilder(testNamer, objectProvider),
				new NUnitSuiteBuilder(testNamer, objectProvider),
				new MSTestSuiteBuilder(testNamer, objectProvider)
			) { }

		SimpleConeRunner(params IConeSuiteBuilder<ConeSuite>[] suiteBuilders) {
			this.suiteBuilders = suiteBuilders;
		}

		public static SimpleConeRunner ConeOnlyRunner(ITestNamer testNamer) {
			return new SimpleConeRunner(new ConePadSuiteBuilder(testNamer, new DefaultFixtureProvider()));
		}

		public int Workers = 1;

		public void RunTests(ICollection<string> runList, TestSession results, IEnumerable<Assembly> assemblies) {
			CheckThat.Batteries.Included();
			results.RunTests(CreateTestRun(
				runList, 
				BuildFlatSuites(assemblies.SelectMany(AssemblyMethods.GetExportedTypes))
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
			RunTests(results, assemblies.SelectMany(AssemblyMethods.GetExportedTypes));

		public void RunTests(TestSession results, IEnumerable<Type> suiteTypes) {
			CheckThat.Batteries.Included();
			var toRun = BuildFlatSuites(suiteTypes)
				.Where(x => results.IncludeSuite(x))
				.ToList();

			var claimed = -1;

			ParameterizedThreadStart runSuite = state => {
				for (var collectResults = (Action<ConeSuite>)state; ; ) {
					var n = Interlocked.Increment(ref claimed);
					if (n >= toRun.Count)
						return;
					collectResults(toRun[n]);
				}
			};

			results.RunSession(collectResults => {
				var workers = new Thread[Workers - 1];
				for (var i = 0; i != workers.Length; ++i) {
					var worker = workers[i] = new Thread(runSuite);
					worker.Start(collectResults);
				}
				runSuite(collectResults);
				workers.ForEach(x => x.Join());
			});
		}

		IEnumerable<ConeSuite> BuildFlatSuites(IEnumerable<Type> suiteTypes) => suiteTypes
			.Choose<Type, ConeSuite>(TryBuildSuite)
			.Flatten(x => x.Subsuites);

		bool TryBuildSuite(Type input, out ConeSuite suite) {
			suite = (suiteBuilders.FirstOrDefault(x => x.SupportedType(input)) ?? NullBuilder)
				.BuildSuite(input);
			return suite != null;
		}
	}
}
