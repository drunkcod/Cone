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

		bool executed, success;
		bool attributesWritten;

		public XmlLogger(XmlWriter xml, IConeTest test) {
			this.xml = xml;
            this.test = test;
			xml.WriteStartElement("test-case");
		}

		public void Failure(ConeTestFailure failure) {		
			executed = true;
			success = false;
			if(FinalizeAttributes()) {
				xml.WriteAttributeString("assembly", new Uri(test.Assembly.Location).LocalPath);
			}

			xml.WriteStartElement("failure");
			xml.WriteAttributeString("context", failure.Context);
			xml.WriteAttributeString("file", failure.File);
			xml.WriteAttributeString("line", failure.Line.ToString(CultureInfo.InvariantCulture));
			xml.WriteAttributeString("column", failure.Column.ToString(CultureInfo.InvariantCulture));
				xml.WriteStartElement("message");
				xml.WriteCData(failure.Message);
				xml.WriteEndElement();
			xml.WriteEndElement();
		}

		public void Success() {
			executed = true;
			success = true;
		}

		public void Pending(string reason) {
			executed = false;
		}

        public void Skipped() { 
			xml.WriteAttributeString("skipped", "True");
		}

		public void EndTest()
		{
			FinalizeAttributes();
			xml.WriteEndElement();
		}

		private bool FinalizeAttributes() {
			if(attributesWritten)
				return false;
			attributesWritten = true;
			
			xml.WriteAttributeString("name", test.TestName.Name);
			xml.WriteAttributeString("context", test.TestName.Context);
			xml.WriteAttributeString("executed", executed.ToString());
			xml.WriteAttributeString("success", success.ToString());

			return true;
		}
	}
}
