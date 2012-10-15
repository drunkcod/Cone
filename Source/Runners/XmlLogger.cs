using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Cone.Core;

namespace Cone.Runners
{
	public class XmlLogger : IConeLogger, ISessionLogger
	{
		readonly XmlWriter xml;

		public XmlLogger(XmlWriter xml) {
			this.xml = xml;
		}

		public void BeginSession() {
			xml.WriteStartDocument();
			xml.WriteStartElement("test-results");
		}

		public void EndSession() {
			xml.WriteEndDocument();
			xml.Flush();
		}

		public void WriteInfo(Action<TextWriter> output) { }

		public void Failure(ConeTestFailure failure) {
			
			xml.WriteStartElement("test-case");
				xml.WriteAttributeString("context", failure.Context);
				xml.WriteAttributeString("name", failure.TestName);
				xml.WriteAttributeString("executed", "True");
				xml.WriteAttributeString("success", "False");
				xml.WriteStartElement("failure");
				xml.WriteAttributeString("file", failure.File);
				xml.WriteAttributeString("line", failure.Line.ToString(CultureInfo.InvariantCulture));
				xml.WriteAttributeString("column", failure.Column.ToString(CultureInfo.InvariantCulture));
					xml.WriteStartElement("message");
					xml.WriteCData(failure.Message);
					xml.WriteEndElement();
				xml.WriteEndElement();
			xml.WriteEndElement();
		}

		public void Success(IConeTest test) {
			xml.WriteStartElement("test-case");
				xml.WriteAttributeString("context", test.Name.Context);
				xml.WriteAttributeString("name", test.Name.Name);
				xml.WriteAttributeString("executed", "True");
				xml.WriteAttributeString("success", "True");
			xml.WriteEndElement();
		}

		public void Pending(IConeTest test) {
			xml.WriteStartElement("test-case");
				xml.WriteAttributeString("context", test.Name.Context);
				xml.WriteAttributeString("name", test.Name.Name);
				xml.WriteAttributeString("executed", "False");
			xml.WriteEndElement();
		}

        public void Skipped(IConeTest test) { }
	}
}
