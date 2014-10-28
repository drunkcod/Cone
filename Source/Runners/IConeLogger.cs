using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
	public interface ISessionWriter
	{
		void Write(string format, params object[] args);
		void WriteHighlight(string format, params object[] args);
		void WriteLine();
		void WriteLine(string format, params object[] args);
	}

	class TextSessionWriter : ISessionWriter
	{
		readonly TextWriter writer;

		public TextSessionWriter(TextWriter writer){
			this.writer = writer;
		}

		public void Write(string format, params object[] args) { writer.Write(format, args); }

		public void WriteHighlight(string format, params object[] args) { Write(format, args); }

		public void WriteLine() { writer.WriteLine(); }

		public void WriteLine(string format, params object[] args) { writer.WriteLine(format, args); }
	}

	class ConsoleSessionWriter : ISessionWriter
	{
		public void Write(string format, params object[] args) { Console.Write(format, args); }

		public void WriteHighlight(string format, params object[] args) {
			lock(Console.Out) {
				var tmp = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Out.Write(format, args);
				Console.ForegroundColor = tmp;
			}
		}

		public void WriteLine() { Console.WriteLine(); }

		public void WriteLine(string format, params object[] args) { Console.WriteLine(format, args); }
	}

	public interface ISessionLogger
	{
		void WriteInfo(Action<ISessionWriter> output);
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
