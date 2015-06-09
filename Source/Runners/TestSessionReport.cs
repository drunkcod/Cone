using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cone.Core;

namespace Cone.Runners
{
	public class TestSessionReport : ISessionLogger, ISuiteLogger, ITestLogger
	{
		int passed;
		int Failed { get { return failures.Count; } }
		int Excluded;
		int Total { get { return passed + Failed + Excluded; } }
		Stopwatch timeTaken;
		readonly List<ConeTestFailure> failures = new List<ConeTestFailure>();

		public int Passed { get { return passed; } }

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

		public void Pending(string reason) { }

		public void Skipped() { Interlocked.Increment(ref Excluded); }

		void ITestLogger.BeginTest() { }

		public void EndTest() { }

		public void WriteReport(ISessionWriter output) {
			output.NewLine();
			output.Info("{0} tests found. {1} Passed. {2} Failed. ({3} Skipped)\n", Total, passed, Failed, Excluded);

			if (failures.Count > 0) {
				output.Write("\nFailures:\n");
				failures.ForEach((n, failure) => {
					output.Write("{0}) ", 1 + n);
					failure.WriteTo(output);
					output.NewLine();
				});
			}
			output.Info("Done in {0}.\n", timeTaken.Elapsed);
		}
	}
}