using Cone.Core;
using Cone.Runners;
using System;
using System.Threading;

namespace Conesole
{

	class ConesoleResultLogger : ISessionLogger, ISuiteLogger, ITestLogger
	{
		public int FailureCount;

		public void WriteInfo(Action<ISessionWriter> output) { }

		public void BeginSession() { }

		public ISuiteLogger BeginSuite(IConeSuite suite) { return this; }

		public void EndSession() { }

		public ITestLogger BeginTest(IConeTest test) { return this; }

		public void EndSuite() { }

		public void Failure(ConeTestFailure failure) {
			Interlocked.Increment(ref FailureCount);
		}

		public void Success() { }

		public void Pending(string reason) { }

		public void Skipped() { }

		void ITestLogger.BeginTest() { }

		public void EndTest() { }
	}
}
