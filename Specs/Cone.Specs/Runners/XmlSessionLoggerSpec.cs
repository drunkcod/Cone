using Cone.Core;
using Cone.Stubs;
using Cone.Xml;
using System;
using System.IO;
using System.Text;
using System.Xml;
using CheckThat;

namespace Cone.Runners
{
	[Describe(typeof(XmlSessionLogger))]
	public class XmlSessionLoggerSpec
	{
		public void setup_failure() {
			var result = new StringBuilder();
			using(var xml = new XmlTextWriter(new StringWriter(result))) {
				var log = new XmlSessionLogger(xml);
				var suiteLog = log.BeginSuite(new ConeSuiteStub());
				var testCase = new ConeTestStub().InContext("Failure").WithName("Setup");
				var testLog = suiteLog.BeginTest(testCase);
				testLog.Failure(new ConeTestFailure(testCase.TestName, new Exception(), FailureType.Setup));
				testLog.TestFinished();
				suiteLog.EndSuite();
			}

			Check.With(() => result.ToString().ReadXml<XmlSessionLoggerTestCase>()).That(
				x => x.Executed == true,
				x => x.Success == false,
				x => x.Duration == null,
				x => x.Failures[0].Type == FailureType.Setup);
		}
	}
}
