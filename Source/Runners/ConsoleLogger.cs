using System;
using System.Diagnostics;
using System.IO;
using Cone.Core;
using System.Collections.Generic;

namespace Cone.Runners
{
	public enum LoggerVerbosity {
		Default,
		TestNames,
		Labels
	}

	class LabledConsoleLoggerContext 
	{
		readonly List<string> parts = new List<string>();

		public int Count { get { return parts.Count; } }

		public string this[int index]{
			get { return parts[index]; }
		}

		public void Set(string[] newContext) {
			parts.Clear();
			parts.AddRange(newContext);
		}
	}

	public class ConsoleLoggerSettings
	{
		readonly internal LabledConsoleLoggerContext Context = new LabledConsoleLoggerContext();
		public LoggerVerbosity Verbosity;
		public ConsoleColor SuccessColor = ConsoleColor.Green;
		public bool Multicore;
		public bool ShowTimings;
	}

	public class ConsoleSessionLogger : ISessionLogger, ISuiteLogger
	{
		readonly ConsoleLoggerSettings settings;
		readonly ConsoleLoggerWriter writer;

		public ConsoleSessionLogger(ConsoleLoggerSettings settings) {
			this.settings = settings;
			switch(settings.Verbosity) {
				case LoggerVerbosity.Default: writer = new ConsoleLoggerWriter(); break;
				case LoggerVerbosity.Labels: writer = new LabledConsoleLoggerWriter(settings.Multicore ? new LabledConsoleLoggerContext() : settings.Context, settings.ShowTimings); break;
				case LoggerVerbosity.TestNames: writer = new TestNameConsoleLoggerWriter(); break;
			}
			writer.InfoColor = Console.ForegroundColor;
			writer.SuccessColor = settings.SuccessColor;
		}

		public void WriteInfo(Action<ISessionWriter> output) {
			output(new ConsoleSessionWriter());
		}

		public void BeginSession() { }

		public ISuiteLogger BeginSuite(IConeSuite suite) {
			return new ConsoleSessionLogger(settings);
		}

		public void EndSuite() { }

		public ITestLogger BeginTest(IConeTest test) {
			return new ConsoleLogger(test, writer);
		}

		public void EndSession() { }
	}

	public class ConsoleResult
	{
		public string Context;
		public string TestName;
		public TestStatus Status;
		public string PendingReason;
		public TimeSpan Duration;
	}

	public class ConsoleLoggerWriter
	{
		public ConsoleColor DebugColor = ConsoleColor.DarkGray;
		public ConsoleColor InfoColor = ConsoleColor.Gray;
		public ConsoleColor SuccessColor = ConsoleColor.Green;
		public ConsoleColor FailureColor = ConsoleColor.Red;
		public ConsoleColor PendingColor = ConsoleColor.Yellow;

		protected void Write(ConsoleColor color, string format, params object[] args) {
			lock(Console.Out) {
				var tmp = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.Out.Write(format, args);
				Console.ForegroundColor = tmp;
			}
		}

		protected void WriteLine() {
			Console.Out.WriteLine();
		}

		public virtual void Write(ConsoleResult result) {
			switch(result.Status) {
				case TestStatus.Success: Write(SuccessColor, "."); break;
				case TestStatus.Pending: Write(PendingColor, "?"); break;
				case TestStatus.TestFailure: Write(FailureColor, "F"); break;
			}
		}
	}

	class LabledConsoleLoggerWriter : ConsoleLoggerWriter
	{	
		readonly LabledConsoleLoggerContext context;
		readonly bool showTimings;

		public LabledConsoleLoggerWriter(LabledConsoleLoggerContext context, bool showTimings) {
			this.context = context;
			this.showTimings = showTimings;
		}

		public override void Write(ConsoleResult result) {
			switch(result.Status) {
				case TestStatus.TestFailure: WriteTestLabel(FailureColor, result.Context, result.TestName); break;
				case TestStatus.Pending: 
					WriteTestLabel(PendingColor, result.Context, result.TestName); 
					if(!string.IsNullOrEmpty(result.PendingReason))
						Write(InfoColor, " \"{0}\"", result.PendingReason);
					break;
				case TestStatus.Success: WriteTestLabel(SuccessColor, result.Context, result.TestName); break;
			}
			if(showTimings)
				Write(DebugColor, " [{0}]", result.Duration);
			WriteLine();
		}

		void WriteTestLabel(ConsoleColor color, string contextName, string testName) {
			var parts = contextName.Split('.');
			var skip = 0;
			while(skip != context.Count && skip != parts.Length && context[skip] == parts[skip])
				++skip;
			context.Set(parts);
			for(; skip != context.Count; ++skip)
				Write(InfoColor, "{0}{1}\n", new string(' ', skip << 1), context[skip]);
			Write(color, "{0}* {1}", new string(' ', skip << 1), testName);
		}
	}

	class TestNameConsoleLoggerWriter : ConsoleLoggerWriter
	{
		public override void Write(ConsoleResult result) {
			switch(result.Status) {
				case TestStatus.TestFailure: WriteTestName(FailureColor, result.Context, result.TestName); break;
				case TestStatus.Pending: WriteTestName(PendingColor, result.Context, result.TestName); break;
				case TestStatus.Success: WriteTestName(SuccessColor, result.Context, result.TestName); break;
			}
		}

		void WriteTestName(ConsoleColor color, string contextName, string testName) {
			Write(color, "{0}.{1}\n", contextName, testName.Replace("\n", "\\n").Replace("\r", ""));
		}
	}

	public class ConsoleLogger : ITestLogger
	{
		readonly IConeTest test;
		readonly ConsoleLoggerWriter writer;
		readonly Stopwatch time;
		bool hasFailed;

		public ConsoleLogger(IConeTest test, ConsoleLoggerWriter writer) {
			this.test = test;
			this.writer = writer;
			this.time = Stopwatch.StartNew();
		}

		public void Failure(ConeTestFailure failure) {
			if(hasFailed)
				return;
			hasFailed = true;
			writer.Write(new ConsoleResult {
				Status = TestStatus.TestFailure,
				Duration = time.Elapsed,
				Context = failure.Context, 
				TestName = failure.TestName
			});
		}

		public void Success() {
			writer.Write(new ConsoleResult {
				Status = TestStatus.Success,
				Duration = time.Elapsed,
				Context = test.TestName.Context,
				TestName = test.Name,
			});
		}

		public void Pending(string reason) {
			writer.Write(new ConsoleResult {
				Status = TestStatus.Pending,
				Duration = time.Elapsed,
				Context = test.TestName.Context,
				TestName = test.Name,
				PendingReason = reason,
			});
		}

		public void Skipped() { }

		public void EndTest() { }
	}
}
