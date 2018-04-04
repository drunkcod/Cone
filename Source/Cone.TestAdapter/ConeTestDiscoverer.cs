using Cone.Runners;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cone.TestAdapter
{
	[ExtensionUri(ConeTestExecutor.ExecutorUriString), FileExtension(".dll"), DefaultExecutorUri(ConeTestExecutor.ExecutorUriString)]
	public class ConeTestDiscoverer : ITestDiscoverer
	{
		public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink) {
			foreach(var source in sources) {
				var xDomainSink = new TestAdapterLogger(logger, source);
				xDomainSink.OnBeginTest += (_, e) => discoverySink.SendTestCase(e.TestCase);
				CrossDomainConeRunner.WithProxyInDomain<ConeTestAdapterProxy, int>(string.Empty, 
					new [] { source, },
					proxy => proxy.DiscoverTests(source, xDomainSink));
			}
		}
	}
}