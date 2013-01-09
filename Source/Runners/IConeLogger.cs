using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
    public interface ISessionLogger
    {
        void WriteInfo(Action<TextWriter> output);
        void BeginSession();
        ISuiteLogger BeginSuite(IConeSuite suite);
        void EndSession();
    }

    public interface ISuiteLogger
    {
        ITestLogger BeginTest(IConeTest test);
        void EndSuite();
    }

    public interface ITestLogger
    {
        void Failure(ConeTestFailure failure);
        void Success();
        void Pending(string reason);
        void Skipped();
		void EndTest();
    }

	public static class LoggerExtensions
	{
		public static void WithTestLog(this ISuiteLogger log, IConeTest test, Action<ITestLogger> action) {
			var testLog = log.BeginTest(test);
			action(testLog);
			testLog.EndTest();
		}
	}
}
