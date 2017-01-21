using System;
using System.Collections.Generic;
using System.Linq;
using Cone.Runners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Cone.TestAdapter
{
	[ExtensionUri(ExecutorUriString)]
	public class ConeTestExecutor : ITestExecutor
	{
		public const string ExecutorUriString = "executor://Cone/TestAdapter/v1";
		public static readonly Uri ExecutorUri = new Uri(ExecutorUriString);

		public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle) {
			foreach(var source in tests.GroupBy(x => x.Source, x => x.FullyQualifiedName)) 
				RunSourceInDomain(source.Key, runContext, frameworkHandle);
		}

		public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle) {
			foreach(var source in sources) 
				RunSourceInDomain(source, runContext, frameworkHandle);
		}

		void RunSourceInDomain(string source, IRunContext runContext, IFrameworkHandle frameworkHandle) { 
			var xDomainSink = CreateLogger(frameworkHandle, source);
			CrossDomainConeRunner.WithProxyInDomain<ConeTestAdapterProxy, int>(string.Empty,  new [] { source },
				proxy => proxy.RunTests(source, xDomainSink));
		}

        TestAdapterLogger CreateLogger(IFrameworkHandle frameworkHandle, string source) {
				var xDomainSink = new TestAdapterLogger(frameworkHandle, source);
				xDomainSink.OnSuccess += (_, e) => frameworkHandle.RecordResult(new TestResult(e.TestCase) { Outcome = TestOutcome.Passed, Duration = e.Duration, });
				xDomainSink.OnPending += (_, e) => frameworkHandle.RecordResult(new TestResult(e.TestCase) { Outcome = TestOutcome.Skipped, Duration = e.Duration });
				xDomainSink.OnFailure += (_, e) => frameworkHandle.RecordResult(new TestResult(e.TestCase) {
					Outcome = TestOutcome.Failed,
					Duration = e.Duration,
					ErrorMessage = e.ErrorMessage,
					ErrorStackTrace = e.ErrorStackTrace,
				});
			return xDomainSink;
		}
		
		public void Cancel() { }
	}
}