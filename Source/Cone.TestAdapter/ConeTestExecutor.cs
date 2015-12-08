using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cone.Runners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Cone.TestAdapter
{
	[ExtensionUri(ExecutorUriString)]
	public class ConeTestExecutor : ITestExecutor
	{
		public const string ExecutorUriString = "executor://Cone/TestAdapter/v1";
		public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);

		public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle) {
			foreach(var source in tests.GroupBy(x => x.Source, x => x.FullyQualifiedName)) {
				var xDomainSink = new TestAdapterLogger(frameworkHandle, source.Key);
				xDomainSink.OnSuccess += (_, e) => frameworkHandle.RecordResult(new TestResult(e.TestCase) { Outcome = TestOutcome.Passed, Duration = e.Duration, });
				xDomainSink.OnPending += (_, e) => frameworkHandle.RecordResult(new TestResult(e.TestCase) { Outcome = TestOutcome.Skipped, Duration = e.Duration });
				xDomainSink.OnFailure += (_, e) => frameworkHandle.RecordResult(new TestResult(e.TestCase) {
					Outcome = TestOutcome.Failed,
					Duration = e.Duration,
					ErrorMessage = e.ErrorMessage,
					ErrorStackTrace = e.ErrorStackTrace,			
				});
				CrossDomainConeRunner.WithProxyInDomain<ConeTestAdapterProxy, int>(string.Empty, 
					new [] { source.Key, },
					proxy => proxy.RunTests(source.Key, xDomainSink, source.ToArray())
				);
			}
		}

		public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle) {
			foreach(var item in sources) 
				frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{item}");
		}

		public void Cancel() { }
	}
}