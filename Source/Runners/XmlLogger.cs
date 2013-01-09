using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Cone.Core;

namespace Cone.Runners
{
    public class XmlSessionLogger : ISessionLogger, ISuiteLogger
    {
        readonly XmlWriter xml;

        public XmlSessionLogger(XmlWriter xml) {
            this.xml = xml;
        }

        public void WriteInfo(Action<TextWriter> output) { }

        public void BeginSession() {
            xml.WriteStartDocument();
            xml.WriteStartElement("test-results");
        }

        public ISuiteLogger BeginSuite(IConeSuite suite) {
            return this;
        }

        public void EndSuite() { }

        public ITestLogger BeginTest(IConeTest test) {
            return new XmlLogger(xml, test);
        }

        public void EndSession() {
            xml.WriteEndDocument();
            xml.Flush();
        }
    }

	public class XmlLogger : ITestLogger
	{
		readonly XmlWriter xml;
        readonly IConeTest test;
		bool isFailing;

		public XmlLogger(XmlWriter xml, IConeTest test) {
			this.xml = xml;
            this.test = test;
			xml.WriteStartElement("test-case");
		}

		public void Failure(ConeTestFailure failure) {		
			if(!isFailing) {
				xml.WriteAttributeString("context", failure.Context);
				xml.WriteAttributeString("assembly", new Uri(test.Assembly.Location).LocalPath);
				xml.WriteAttributeString("name", failure.TestName);
				xml.WriteAttributeString("executed", "True");
				xml.WriteAttributeString("success", "False");
			}
			xml.WriteStartElement("failure");
			xml.WriteAttributeString("file", failure.File);
			xml.WriteAttributeString("line", failure.Line.ToString(CultureInfo.InvariantCulture));
			xml.WriteAttributeString("column", failure.Column.ToString(CultureInfo.InvariantCulture));
				xml.WriteStartElement("message");
				xml.WriteCData(failure.Message);
				xml.WriteEndElement();
			xml.WriteEndElement();

			isFailing = true;
		}

		public void Success() {
			xml.WriteAttributeString("context", test.TestName.Context);
			xml.WriteAttributeString("name", test.TestName.Name);
			xml.WriteAttributeString("executed", "True");
			xml.WriteAttributeString("success", "True");
		}

		public void Pending(string reason) {
			xml.WriteAttributeString("context", test.TestName.Context);
			xml.WriteAttributeString("name", test.TestName.Name);
			xml.WriteAttributeString("executed", "False");
		}

        public void Skipped() { 
			xml.WriteAttributeString("context", test.TestName.Context);
			xml.WriteAttributeString("name", test.TestName.Name);
			xml.WriteAttributeString("executed", "False");
			xml.WriteAttributeString("skipped", "True");

		}

		public void EndTest() {
			xml.WriteEndElement();
		}
	}
}
