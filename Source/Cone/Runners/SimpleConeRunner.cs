﻿using Cone.Core;
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

		public SimpleConeRunner(): this(new DefaultFixtureProvider())
		{ }

		public SimpleConeRunner(FixtureProvider objectProvider) : this(
				new ConePadSuiteBuilder(objectProvider),
				new NUnitSuiteBuilder(objectProvider),
				new MSTestSuiteBuilder(objectProvider)
			) { }

		SimpleConeRunner(params IConeSuiteBuilder<ConePadSuite>[] suiteBuilders) {
			this.suiteBuilders = suiteBuilders;
		}

		public static SimpleConeRunner ConeOnlyRunner() {
			return new SimpleConeRunner(new ConePadSuiteBuilder(new DefaultFixtureProvider()));
		}

		public int Workers = 1;

		public void RunTests(ICollection<string> runList, TestSession results, IEnumerable<Assembly> assemblies) {
			var runOrder = new Dictionary<string, int>();
			var n = 0;
			foreach (var item in runList)
				runOrder.Add(item, n++);
			var found = new int[runList.Count + 1];
			n = 0;
			var toRun = assemblies.SelectMany(x => x.GetExportedTypes())
				.Choose<Type, ConePadSuite>(TryBuildSuite)
				.Flatten(x => x.Subsuites)
				.SelectMany(x => x.Tests)
				.Where(x => {
					var r = runOrder.TryGetValue(x.Name, out found[n]);
					if(r)
						++n;
					return r;
				})
				.ToArray();
			Array.Sort(found, toRun, 0, n);
			Check.Initialize();
			results.RunTests(toRun);
		}

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
		}

		bool TryBuildSuite(Type input, out ConePadSuite suite) {
			suite = (suiteBuilders.FirstOrDefault(x => x.SupportedType(input)) ?? NullBuilder)
				.BuildSuite(input);
			return suite != null;
		}
	}
}
