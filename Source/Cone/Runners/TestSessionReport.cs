using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cone.Core;
using CheckThat.Internals;

namespace Cone.Runners
{
	public class TestSessionReport : ISessionLogger, ISuiteLogger, ITestLogger
	{
		int passed, pending;
		int excluded;
		Stopwatch timeTaken;

		readonly List<ConeTestFailure> failures = new List<ConeTestFailure>();

		public int Passed => passed;
		public int Failed => failures.Count;
		public int Pending => pending;
		public int Excluded => excluded;
		public int Total => passed + Failed + Excluded;

		public void BeginSession() {
			timeTaken = Stopwatch.StartNew();
		}

		public void EndSession() {
			timeTaken.Stop();
		}

		public ISuiteLogger BeginSuite(IConeSuite suite) => this;

		public void EndSuite() { }

		public ITestLogger BeginTest(IConeTest test) => this;

		public void WriteInfo(Action<ISessionWriter> output) { }

		public void Success() { Interlocked.Increment(ref passed); }

		public void Failure(ConeTestFailure failure) { lock(failures) failures.Add(failure); }

		void ITestLogger.Pending(string reason) { ++pending; }

		public void Skipped() { Interlocked.Increment(ref excluded); }

		void ITestLogger.TestStarted() { }

		public void TestFinished() { }

		public void WriteReport(ISessionWriter output) {
			output.Info("\n{0} tests found. {1} Passed. {2} Failed. ({3} Skipped)\n", Total, passed, Failed, excluded);

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