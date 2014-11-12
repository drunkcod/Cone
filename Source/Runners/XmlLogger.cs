using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Cone.Core;
using System.Diagnostics;
using System.Threading;

namespace Cone.Runners
{		
	public class XmlSessionSummary
	{
		public int Passed;
		public int Failed;
		public int Pending;
		public int Skipped;
		public readonly Stopwatch Duration = Stopwatch.StartNew();
	}

	public class XmlSessionLogger : ISessionLogger
	{
		readonly XmlWriter xml;
		readonly XmlSessionSummary summary = new XmlSessionSummary();

		class XmlSuiteLogger : ISuiteLogger
		{
			static XmlWriterSettings WriterSettings = new XmlWriterSettings { 
				ConformanceLevel = ConformanceLevel.Fragment,
			};

			static XmlReaderSettings ReaderSettings = new XmlReaderSettings {
				ConformanceLevel = ConformanceLevel.Fragment,
			};

			readonly XmlSessionLogger session;
			readonly MemoryStream suite;

			public XmlSuiteLogger(XmlSessionLogger session) {
				this.session = session;
				this.suite = new MemoryStream();
			}

			public ITestLogger BeginTest(IConeTest test) {
				return new XmlLogger(session.summary, XmlWriter.Create(suite, WriterSettings), test);
			}

			public void EndSuite() {
				suite.Position = 0;
				session.WriteNode(XmlReader.Create(suite, ReaderSettings));
			}
		}

		public XmlSessionLogger(XmlWriter xml) {
			this.xml = xml;
		}

		public EventHandler SessionEnded;
		
		public void WriteInfo(Action<ISessionWriter> output) { }

		public void BeginSession() {
			xml.WriteStartDocument();
			xml.WriteStartElement("test-results");
			xml.WriteAttributeString("started-at", DateTime.Now.ToString("o"));
			xml.WriteAttributeString("host", Environment.MachineName);
			xml.WriteAttributeString("user", Environment.UserName);
		}

		public ISuiteLogger BeginSuite(IConeSuite suite) {
			return new XmlSuiteLogger(this);
		}

		public void EndSession() {
			xml.WriteStartElement("summary");
				xml.WriteAttributeString("total-duration", summary.Duration.Elapsed.ToString());
				xml.WriteAttributeString("passed", summary.Passed.ToString());
				xml.WriteAttributeString("failed", summary.Failed.ToString());
				xml.WriteAttributeString("pending", summary.Pending.ToString());
				xml.WriteAttributeString("skipped", summary.Skipped.ToString());
			xml.WriteEndDocument();
			xml.Flush();
			SessionEnded.Raise(this, EventArgs.Empty);
		}

		void WriteNode(XmlReader reader) {
			lock(xml)
				xml.WriteNode(reader, true);
		}
	}

	public class XmlLogger : ITestLogger
	{
		readonly XmlWriter xml;
		readonly IConeTest test;
		readonly Stopwatch duration = Stopwatch.StartNew();
		readonly XmlSessionSummary summary;

		bool executed, success;
		bool attributesWritten;

		public XmlLogger(XmlSessionSummary summary, XmlWriter xml, IConeTest test) {
			this.summary = summary;
			this.xml = xml;
			this.test = test;
			xml.WriteStartElement("test-case");
		}

		public void Failure(ConeTestFailure failure) {
			summary.Failed += 1;
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
			Interlocked.Increment(ref summary.Passed);
			executed = true;
			success = true;
		}

		public void Pending(string reason) {
			Interlocked.Increment(ref summary.Pending);
			executed = false;
		}

		public void Skipped() { 
			Interlocked.Increment(ref summary.Skipped);
			xml.WriteAttributeString("skipped", "True");
		}

		public void EndTest() {
			FinalizeAttributes();
			xml.WriteEndElement();
			xml.Flush();
		}

		private bool FinalizeAttributes() {
			if(attributesWritten)
				return false;
			attributesWritten = true;
			
			xml.WriteAttributeString("name", test.TestName.Name);
			xml.WriteAttributeString("context", test.TestName.Context);
			xml.WriteAttributeString("executed", executed.ToString());
			xml.WriteAttributeString("success", success.ToString());
			xml.WriteAttributeString("duration", duration.Elapsed.ToString());

			return true;
		}
	}
}
