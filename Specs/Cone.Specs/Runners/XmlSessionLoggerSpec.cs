using Cone.Core;
using Cone.Stubs;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Cone.Runners
{
	[Describe(typeof(XmlSessionLogger))]
	public class XmlSessionLoggerSpec
	{
		public void setup_failure() {
			var result = new MemoryStream();
			var xml = new XmlTextWriter(result, Encoding.UTF8);
			var log = new XmlSessionLogger(xml);

			var suiteLog = log.BeginSuite(new ConeSuiteStub());
			var testCase = new ConeTestStub().InContext("Failure").WithName("Setup");
			var testLog = suiteLog.BeginTest(testCase);
			testLog.Failure(new ConeTestFailure(testCase.TestName, new Exception(), FailureType.Setup));
			testLog.TestFinished();
			suiteLog.EndSuite();
			xml.Flush();
			result.Position = 0;
			Check.With(() => ParseTestCase(new StreamReader(result, Encoding.UTF8))).That(
				x => x.Executed == true,
				x => x.Success == false,
				x => x.Duration == null,
				x => x.Failures[0].Type == FailureType.Setup);
		}

		XmlSessionLoggerTestCase ParseTestCase(TextReader input) {
			var xml = new XmlSerializer(typeof(XmlSessionLoggerTestCase));
			return (XmlSessionLoggerTestCase)xml.Deserialize(input);
		}
	}
}
