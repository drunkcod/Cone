using System;
using System.Globalization;
using System.IO;
using System.Xml;
using Cone.Core;
using System.Diagnostics;
using System.Threading;
using System.Xml.Serialization;

namespace Cone.Runners
{		
	static class XmlWriterExtensions
	{
		public static void WriteAttribute(this XmlWriter xml, string name, bool value) {
			xml.WriteStartAttribute(name);
			xml.WriteValue(value);
			xml.WriteEndAttribute();
		}
	}

	public class XmlSessionSummary
	{
		public int Passed;
		public int Failed;
		public int Pending;
		public int Skipped;
		public readonly Stopwatch Duration = Stopwatch.StartNew();
	}

	[XmlRoot("test-case")]
	public class XmlSessionLoggerTestCase
	{
		[XmlAttribute("executed")]
		public bool Executed;
		[XmlAttribute("success")]
		public bool Success;
		[XmlAttribute("duration")]
		public string Duration;

		[XmlElement("failure")]
		public XmlSessionLoggerFailure[] Failures;
	}

	public class XmlSessionLoggerFailure
	{
		[XmlAttribute("type")]
		public FailureType Type;
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
				var xml = XmlWriter.Create(suite, WriterSettings);
				xml.WriteStartElement("test-case");
				return new XmlTestLogger(session.summary, xml, test);
			}

			public void EndSuite() {
				suite.Position = 0;
				session.WriteNode(XmlReader.Create(suite, ReaderSettings));
			}
		}

		class XmlTestLogger : ITestLogger
		{
			readonly XmlWriter xml;
			readonly IConeTest test;
			readonly XmlSessionSummary summary;
			Stopwatch duration;

			bool executed, success;
			bool attributesWritten;

			public XmlTestLogger(XmlSessionSummary summary, XmlWriter xml, IConeTest test) {
				this.summary = summary;
				this.xml = xml;
				this.test = test;
			}

			public void TestStarted() {
				duration = Stopwatch.StartNew();
			}

			public void Failure(ConeTestFailure failure) {
				summary.Failed += 1;
				executed = true;
				success = false;
				if(FinalizeAttributes()) {
					xml.WriteAttributeString("assembly", new Uri(test.Assembly.Location).LocalPath);
				}

				xml.WriteStartElement("failure");
					xml.WriteAttributeString("type", failure.FailureType.ToString());
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
				xml.WriteAttribute("skipped", true);
			}

			public void TestFinished() {
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
				xml.WriteAttribute("executed", executed);
				xml.WriteAttribute("success", success);
				if(duration != null)
					xml.WriteAttributeString("duration", duration.Elapsed.ToString());

				return true;
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
}
