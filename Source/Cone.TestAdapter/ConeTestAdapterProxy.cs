using System;
using System.Collections.Generic;
using Cone.Core;
using Cone.Runners;

namespace Cone.TestAdapter
{
	class ConeTestAdapterProxy : MarshalByRefObject
	{
		readonly SimpleConeRunner runner = SimpleConeRunner.ConeOnlyRunner(new BasicTestNamer());

		public int DiscoverTests(string source, ICrossDomainLogger sink) {
			var dryRun = new DryRunTestExecutor();
			var session = new TestSession(new CrossDomainSessionLoggerAdapter(sink)) {
				GetTestExecutor = _ => dryRun,
			};
			return RunSession(source, sink, session);
		}

		public int RunTests(string source, ICrossDomainLogger sink) {
			var session = new TestSession(new CrossDomainSessionLoggerAdapter(sink));
			return RunSession(source, sink, session);
		}

        public int RunTests(string source, ICrossDomainLogger sink, IEnumerable<string> tests) {
			var testsToRun = new HashSet<string>(tests);
			var session = new TestSession(new CrossDomainSessionLoggerAdapter(sink)) {
				ShouldSkipTest = test => !testsToRun.Contains(test.Name),
			};
			return RunSession(source, sink, session);
		}

		private int RunSession(string source, ICrossDomainLogger sink, TestSession session) {
			runner.RunTests(session, CrossDomainConeRunner.LoadTestAssemblies(new[] {source}, sink.Error));
			return 0;
		}
	}
}