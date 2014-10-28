using System;
using System.IO;

namespace Cone.Runners
{
	public class NullLogger : ISessionLogger, ISuiteLogger, ITestLogger
	{
		public void BeginSession() { }

		public ITestLogger BeginTest(Core.IConeTest test) {
			return this;
		}

		public ISuiteLogger BeginSuite(Core.IConeSuite suite) {
			return this;
		}

		public void EndSuite() { }
		public void EndSession() { }
		public void WriteInfo(Action<ISessionWriter> output) { }
		public void Failure(ConeTestFailure failure) { }
		public void Success() { }
		public void Pending(string reason) { }
		public void Skipped() { }
		public void EndTest() { }
	}}
