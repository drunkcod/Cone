using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cone.Core;
using Cone.Runners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Cone.TestAdapter
{
	public class TestAdapterEventArgs : EventArgs
	{
		public TestCase TestCase;
		public TimeSpan Duration;
		public string ErrorMessage;
		public string ErrorStackTrace;
	}

	class TestAdapterLogger : MarshalByRefObject, ICrossDomainLogger
	{
		readonly string source;
		readonly IMessageLogger logger;

		TestCase currentTestCase;
		Stopwatch stopwatch;

		public EventHandler<TestAdapterEventArgs> OnBeginTest; 
		public EventHandler<TestAdapterEventArgs> OnSuccess; 
		public EventHandler<TestAdapterEventArgs> OnFailure; 
		public EventHandler<TestAdapterEventArgs> OnPending; 

		public TestAdapterLogger(IMessageLogger logger, string source) {
			this.source = source;
			this.logger = logger;
		}

		public void Info(string message){ logger.SendMessage(TestMessageLevel.Informational, message); }
		public void Error(string message) { logger.SendMessage(TestMessageLevel.Error, message); }

		public void Success() { OnSuccess?.Invoke(this, new TestAdapterEventArgs { TestCase = currentTestCase, Duration = stopwatch.Elapsed }); }
		public void Failure(string file, int line, int column, string message, string stackTrace) {
			currentTestCase.LineNumber = line;
			currentTestCase.CodeFilePath = file;
			OnFailure?.Invoke(this, new TestAdapterEventArgs {
				TestCase = currentTestCase,
				Duration = stopwatch.Elapsed,
				ErrorMessage = message,
				ErrorStackTrace = stackTrace,
			});
		}
		public void Pending(string reason) { OnPending?.Invoke(this, new TestAdapterEventArgs { TestCase = currentTestCase, Duration = stopwatch.Elapsed  }); }

		public void BeginTest(ConeTestName test) {
			currentTestCase = new TestCase(test.FullName, ConeTestExecutor.ExecutorUri, source) {
				DisplayName = test.Name,
			};
			currentTestCase.Traits.Add("Context", test.Context);
			OnBeginTest?.Invoke(this, new TestAdapterEventArgs { TestCase = currentTestCase });
			stopwatch = Stopwatch.StartNew();
		}
	}

	[FileExtension(".dll"), DefaultExecutorUri(ConeTestExecutor.ExecutorUriString)]
	public class ConeTestDiscoverer : ITestDiscoverer
	{
		public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink) {
			foreach(var source in sources) {
				var xDomainSink = new TestAdapterLogger(logger, source);
				xDomainSink.OnBeginTest += (_, e) => discoverySink.SendTestCase(e.TestCase);
				CrossDomainConeRunner.WithProxyInDomain<ConeTestAdapterProxy, int>(string.Empty, 
					new [] { source, },
					proxy => proxy.DiscoverTests(source, xDomainSink)
				);
			}
		}
	}
}
