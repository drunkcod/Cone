using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cone.Core;

namespace Cone.Runners
{
	public class TestSessionReport : ISessionLogger, ISuiteLogger, ITestLogger
	{
		int passed, pending;

		int Failed { get { return failures.Count; } }
		int Excluded;
		int Total { get { return passed + Failed + Excluded; } }
		Stopwatch timeTaken;

		readonly List<ConeTestFailure> failures = new List<ConeTestFailure>();

		public int Passed => passed;
		public int Pending => pending;

		public void BeginSession() {
			timeTaken = Stopwatch.StartNew();
		}

		public void EndSession() {
			timeTaken.Stop();
		}

		public ISuiteLogger BeginSuite(IConeSuite suite) {
			return this;
		}

		public void EndSuite() { }

		public ITestLogger BeginTest(IConeTest test) {
			return this;
		}

		public void WriteInfo(Action<ISessionWriter> output) { }

		public void Success() { Interlocked.Increment(ref passed); }

		public void Failure(ConeTestFailure failure) { lock(failures) failures.Add(failure); }

		void ITestLogger.Pending(string reason) { ++pending; }

		public void Skipped() { Interlocked.Increment(ref Excluded); }

		void ITestLogger.TestStarted() { }

		public void TestFinished() { }

		public void WriteReport(ISessionWriter output) {
			output.Info("\n{0} tests found. {1} Passed. {2} Failed. ({3} Skipped)\n", Total, passed, Failed, Excluded);

			if (failures.Count > 0) {
				output.Write("\nFailures:\n");
				failures.ForEach((n, failure) => {
					output.Write("{0}) ", 1 + n);
					failure.WriteTo(output);
					output.Write("\n");
				});
			}
			output.Info("Done in {0}.\n", timeTaken.Elapsed);
		}
	}
}