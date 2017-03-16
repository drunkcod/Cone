using Cone.Core;
using System;
using System.Diagnostics;
using System.Threading;

namespace Cone.Runners
{
	public enum LoggerVerbosity {
		Default,
		TestNames,
		Labels
	}

	class LabledConsoleLoggerContext 
	{
		string[] parts = new string[0];

		public int Count => parts.Length;

		public string this[int index] => parts[index];

		public void Set(string[] newContext) => 
			parts = newContext;
	}

	public class ConsoleLoggerSettings
	{
		public LoggerVerbosity Verbosity;
		public ConsoleColor SuccessColor = ConsoleColor.Green;
		public bool Multicore;
		public bool ShowTimings;
	}

	class MultiCoreConsoleResultWriter : IConsoleResultWriter
	{
		readonly CircularQueue<ConsoleResult> results = new CircularQueue<ConsoleResult>(8);
		readonly AutoResetEvent resultsAvailable = new AutoResetEvent(false);
		readonly Thread worker;
		bool jobsDone;

		public static IConsoleResultWriter For(IConsoleResultWriter writer) =>
			new MultiCoreConsoleResultWriter(writer);

		MultiCoreConsoleResultWriter(IConsoleResultWriter inner) {
			worker = new Thread(() => {
				ConsoleResult item;
				while(resultsAvailable.WaitOne() && !jobsDone)
					while(results.TryDeque(out item))
						inner.Write(item);
			});

			worker.Start();
		}

		public void Write(ConsoleResult result) {
			while(!results.TryEnqueue(result)) {
				Thread.Sleep(0);
			}
			resultsAvailable.Set();
		}

		public void Close() { 
			jobsDone = true;
			resultsAvailable.Set();
			worker.Join();
		}
	}

	public class ConsoleSessionLogger : ISessionLogger
	{
		readonly ConsoleSessionWriter sessionWriter = new ConsoleSessionWriter();
		readonly ConsoleLoggerSettings settings;
		readonly IConsoleResultWriter writer;
		readonly ISuiteLogger suiteLogger;

		class ConsoleSuiteLogger : ISuiteLogger
		{
			readonly IConsoleResultWriter writer;

			public ConsoleSuiteLogger(IConsoleResultWriter writer) {
				this.writer = writer;
			}

			public void EndSuite() { }

			public ITestLogger BeginTest(IConeTest test) {
				return new ConsoleLogger(test, writer);
			}
		}

		public ConsoleSessionLogger(ConsoleLoggerSettings settings) {
			this.settings = settings;
			this.writer = CreateBaseWriter(settings);
			this.suiteLogger = new ConsoleSuiteLogger(this.writer);
		}

		private static IConsoleResultWriter CreateBaseWriter(ConsoleLoggerSettings settings) {
			ConsoleLoggerWriter writer = null;
			switch(settings.Verbosity) {
				case LoggerVerbosity.Default: writer = new ConsoleLoggerWriter(); break;
				case LoggerVerbosity.Labels: writer = new LabledConsoleLoggerWriter(new LabledConsoleLoggerContext(), settings.ShowTimings); break;
				case LoggerVerbosity.TestNames: writer = new TestNameConsoleLoggerWriter(); break;
			}
			writer.Configuration.InfoColor = Console.ForegroundColor;
			writer.Configuration.SuccessColor = settings.SuccessColor;
			return settings.Multicore ? MultiCoreConsoleResultWriter.For(writer) : writer;
		}

		public void WriteInfo(Action<ISessionWriter> output) {
			output(sessionWriter);
		}

		public void BeginSession() { }

		public ISuiteLogger BeginSuite(IConeSuite suite) {
			return suiteLogger;
		}

		public void EndSession() { writer.Close(); }
	}

	public class ConsoleResult
	{
		public ConsoleResult(IConeTest test) {
			this.Context = test.TestName.Context;
			this.TestName = test.TestName.Name;
		}

		public ConsoleResult(ConeTestFailure failure) {
			this.Context = failure.Context;
			this.TestName = failure.TestName;
		}

		public readonly string Context;
		public readonly string TestName;
		public TestStatus Status;
		public string PendingReason;
		public TimeSpan Duration;
	}

	public interface IConsoleResultWriter 
	{
		void Write(ConsoleResult result);
		void Close();
	}

	public class ConsoleWriterConfiguration
	{
		public ConsoleColor DebugColor = ConsoleColor.DarkGray;
		public ConsoleColor InfoColor = ConsoleColor.Gray;
		public ConsoleColor SuccessColor = ConsoleColor.Green;
		public ConsoleColor FailureColor = ConsoleColor.Red;
		public ConsoleColor PendingColor = ConsoleColor.Yellow;

		public char PassMarker = '√';
		public char PendingMarker = '?';
		public char FailMarker = '×';

		public static ConsoleWriterConfiguration Create() => Console.IsOutputRedirected
			? new ConsoleWriterConfiguration { PassMarker = '.', FailMarker = 'F' }
			: new ConsoleWriterConfiguration();
	}

	public class ConsoleLoggerWriter : IConsoleResultWriter
	{
		public ConsoleWriterConfiguration Configuration = ConsoleWriterConfiguration.Create();

		protected void Write(ConsoleColor color, string format, params object[] args) {
			var message = string.Format(format, args);
			var tmp = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Out.Write(message);
			Console.ForegroundColor = tmp;
		}

		protected void WriteLine() => Console.Out.WriteLine();

		public virtual void Write(ConsoleResult result) {
			switch(result.Status) {
				case TestStatus.Success: Write(Configuration.SuccessColor, "{0}", Configuration.PassMarker); break;
				case TestStatus.Pending: Write(Configuration.PendingColor, "{0}", Configuration.PendingMarker); break;
				case TestStatus.TestFailure: Write(Configuration.FailureColor, "{0}", Configuration.FailMarker); break;
			}
		}

		void IConsoleResultWriter.Close() { }
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
				case TestStatus.TestFailure: WriteTestLabel(Configuration.FailureColor, result.Context, result.TestName, Configuration.FailMarker); break;
				case TestStatus.Pending: 
					WriteTestLabel(Configuration.PendingColor, result.Context, result.TestName, Configuration.PendingMarker); 
					if(!string.IsNullOrEmpty(result.PendingReason))
						Write(Configuration.InfoColor, " \"{0}\"", result.PendingReason);
					break;
				case TestStatus.Success: WriteTestLabel(Configuration.SuccessColor, result.Context, result.TestName, Configuration.PendingMarker); break;
			}
			if(showTimings)
				Write(Configuration.DebugColor, " [{0}]", result.Duration);
			WriteLine();
		}

		void WriteTestLabel(ConsoleColor color, string contextName, string testName, char marker) {
			var parts = contextName.Split('.');
			var skip = 0;
			while(skip != context.Count && skip != parts.Length && context[skip] == parts[skip])
				++skip;
			context.Set(parts);
			for(; skip != context.Count; ++skip)
				Write(Configuration.InfoColor, "{0}{1}\n", new string(' ', skip << 1), context[skip]);
			Write(color, "{0}{2} {1}", new string(' ', skip << 1), testName, marker);
		}
	}

	class TestNameConsoleLoggerWriter : ConsoleLoggerWriter
	{
		public override void Write(ConsoleResult result) {
			switch(result.Status) {
				case TestStatus.TestFailure: WriteTestName(Configuration.FailureColor, result.Context, result.TestName); break;
				case TestStatus.Pending: WriteTestName(Configuration.PendingColor, result.Context, result.TestName); break;
				case TestStatus.Success: WriteTestName(Configuration.SuccessColor, result.Context, result.TestName); break;
			}
		}

		void WriteTestName(ConsoleColor color, string contextName, string testName) {
			Write(color, "{0}.{1}\n", contextName, testName.Replace("\n", "\\n").Replace("\r", ""));
		}
	}

	public class ConsoleLogger : ITestLogger
	{
		readonly IConeTest test;
		readonly IConsoleResultWriter writer;
		readonly Stopwatch time;
		bool hasFailed;

		public ConsoleLogger(IConeTest test, IConsoleResultWriter writer) {
			this.test = test;
			this.writer = writer;
			this.time = new Stopwatch();
		}

		public void Failure(ConeTestFailure failure) {
			if(hasFailed)
				return;
			hasFailed = true;
			writer.Write(new ConsoleResult(failure) {
				Status = TestStatus.TestFailure,
				Duration = time.Elapsed,
			});
		}

		public void Success() {
			writer.Write(new ConsoleResult(test) {
				Status = TestStatus.Success,
				Duration = time.Elapsed,
			});
		}

		public void Pending(string reason) {
			writer.Write(new ConsoleResult(test) {
				Status = TestStatus.Pending,
				Duration = time.Elapsed,
				PendingReason = reason,
			});
		}

		public void Skipped() { }

		public void TestStarted() => time.Start();
		public void TestFinished() => time.Stop();
	}
}
