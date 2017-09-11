using Cone.Core;
using Cone.Stubs;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Cone.Runners
{
	[Describe(typeof(XmlSessionLogger))]
	public class XmlSessionLoggerSpec
	{
		public void setup_failure() {
			var result = new StringWriter();
			var xml = new XmlTextWriter(result);
			var log = new XmlSessionLogger(xml);

			var suiteLog = log.BeginSuite(new ConeSuiteStub());
			var testCase = new ConeTestStub().InContext("Failure").WithName("Setup");
			var testLog = suiteLog.BeginTest(testCase);
			testLog.Failure(new ConeTestFailure(testCase.TestName, new Exception(), FailureType.Setup));
			testLog.TestFinished();
			suiteLog.EndSuite();
			Check.With(() => XDocument.Parse(result.ToString()).Root).That(
				x => x.Name == "test-case",
				x => x.Attribute("executed").Value == "True",
				x => x.Attribute("success").Value == "False",
				x => x.Element("failure").Attribute("type").Value == "Setup",
				x => x.Attributes().All(attr => attr.Name != "duration"));
		}
	}
}
