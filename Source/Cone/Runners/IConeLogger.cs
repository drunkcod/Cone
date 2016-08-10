using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
	public interface ISessionWriter
	{
		void Write(string format, params object[] args);
		void Important(string format, params object[] args);
		void Info(string format, params object[] args);
		void NewLine();
	}

	public class TextSessionWriter : ISessionWriter
	{
		readonly TextWriter writer;

		public TextSessionWriter(TextWriter writer){
			this.writer = writer;
		}

		public void Write(string format, params object[] args) { writer.Write(format, args); }

		public void Important(string format, params object[] args) { Write(format, args); }

		public void Info(string format, params object[] args) { Write(format, args); }

		public void NewLine() { writer.WriteLine(); }
	}

	class ConsoleSessionWriter : ISessionWriter
	{
		public const ConsoleColor ImportantColor = ConsoleColor.Yellow;
		public const ConsoleColor InfoColor = ConsoleColor.Cyan;

		public void Write(string format, params object[] args) { Console.Write(format, args); }

		public void Important(string format, params object[] args) { ColorWrite(ImportantColor, format, args); }

		public void Info(string format, params object[] args) { ColorWrite(InfoColor, format, args);	}
		
		private static void ColorWrite(ConsoleColor color, string format, object[] args) {
			lock(Console.Out) {
				var tmp = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.Out.Write(format, args);
				Console.ForegroundColor = tmp;
			}
		}

		public void NewLine() { Console.WriteLine(); }
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
		void BeginTest();
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
