using Cone.Runners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cone.TestAdapter
{
	[DefaultExecutorUri(ConeTestExecutor.ExecutorUriString)]
	[FileExtension(".dll")]
	public class ConeTestDiscoverer : ITestDiscoverer
	{
		public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink) {
			foreach (var source in sources)
			try { 
				logger.SendMessage(TestMessageLevel.Informational, $"Checking {source}");
				var xDomainSink = new TestAdapterLogger(logger, source);
				xDomainSink.OnBeginTest += (_, e) => discoverySink.SendTestCase(e.TestCase);
				CrossDomainConeRunner.WithProxyInDomain<ConeTestAdapterProxy, int>(string.Empty, 
					new [] { source, },
					proxy => proxy.DiscoverTests(source, xDomainSink));
			} catch(Exception ex) { 
				logger.SendMessage(TestMessageLevel.Error, "Failed to discover tests in " + source + "\n" + ex);
			}
		}
	}
}