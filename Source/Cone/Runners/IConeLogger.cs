using Cone.Core;
using System;
using System.IO;

namespace Cone.Runners
{
	public class TextSessionWriter : ISessionWriter
	{
		readonly TextWriter writer;

		public TextSessionWriter(TextWriter writer){
			this.writer = writer;
		}

		public void Write(string message) => writer.Write(message);
		public void Important(string message) => Write(message);
		public void Info(string message) => Write(message);
		
		public void Write(string format, params object[] args) => writer.Write(format, args);
		public void Important(string format, params object[] args) => Write(format, args);
		public void Info(string format, params object[] args) => Write(format, args);
	}

	class ConsoleSessionWriter : ISessionWriter
	{
		public const ConsoleColor ImportantColor = ConsoleColor.Yellow;
		public const ConsoleColor InfoColor = ConsoleColor.Cyan;

		public void Write(string message) => Console.Write(message);
		public void Important(string message) => ColorWrite(ImportantColor, x => x.Write(message));
		public void Info(string message) => ColorWrite(InfoColor, x => x.Write(message));

		public void Write(string format, params object[] args) => Console.Write(format, args);
		public void Important(string format, params object[] args) => ColorWrite(ImportantColor, x => x.Write(format, args));
		public void Info(string format, params object[] args) => ColorWrite(InfoColor, x => x.Write(format, args));
		
		private static void ColorWrite(ConsoleColor color, Action<TextWriter> doWrite) {
			lock(Console.Out) {
				var tmp = Console.ForegroundColor;
				Console.ForegroundColor = color;
				doWrite(Console.Out);				
				Console.ForegroundColor = tmp;
			}
		}
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
		void TestStarted();
		void Failure(ConeTestFailure failure);
		void Success();
		void Pending(string reason);
		void Skipped();
		void TestFinished();
	}

	public static class LoggerExtensions
	{
		public static void WithTestLog(this ISuiteLogger log, IConeTest test, Action<ITestLogger> action) {
			var testLog = log.BeginTest(test);
			action(testLog);
			testLog.TestFinished();
		}
	}
}
