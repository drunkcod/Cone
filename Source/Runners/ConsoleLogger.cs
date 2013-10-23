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

    public class ConsoleLoggerSettings
    {
        readonly internal List<string> Context = new List<string>();
        public LoggerVerbosity Verbosity;
        public ConsoleColor SuccessColor = ConsoleColor.Green;
		public bool Multicore;
    }

    public class ConsoleSessionLogger : ISessionLogger, ISuiteLogger
    {
		readonly ConsoleLoggerSettings settings;
		readonly ConsoleLoggerWriter writer;

		public ConsoleSessionLogger(ConsoleLoggerSettings settings) {
			this.settings = settings;
			switch(settings.Verbosity) {
				case LoggerVerbosity.Default: writer = new ConsoleLoggerWriter(); break;
				case LoggerVerbosity.Labels: writer = new LabledConsoleLoggerWriter(settings.Multicore ? new List<string>() : settings.Context); break;
				case LoggerVerbosity.TestNames: writer = new TestNameConsoleLoggerWriter(); break;
			}
			writer.InfoColor = Console.ForegroundColor;
			writer.SuccessColor = settings.SuccessColor;
		}

        public void WriteInfo(Action<TextWriter> output) {
            output(Console.Out);
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

	public class ConsoleLoggerWriter
	{
		public ConsoleColor InfoColor = ConsoleColor.Gray;
        public ConsoleColor SuccessColor = ConsoleColor.Green;
		public ConsoleColor FailureColor = ConsoleColor.Red;
		public ConsoleColor PendingColor = ConsoleColor.Yellow;

		public void Write(ConsoleColor color, string format, params object[] args) {
			lock(Console.Out) {
				var tmp = Console.ForegroundColor;
				Console.ForegroundColor = color;
				Console.Out.Write(format, args);
				Console.ForegroundColor = tmp;
			}
        }

		public virtual void WriteFailure(string context, string testName) {
			Write(FailureColor, "F");
		}

		public virtual void WriteSuccess(IConeTest test) {
			Write(SuccessColor, ".");
		}

		public virtual void WritePending(IConeTest test) {
			Write(SuccessColor, "?");
		}
	}

	class LabledConsoleLoggerWriter : ConsoleLoggerWriter
	{
		readonly List<string> context;

		public LabledConsoleLoggerWriter(List<string> context) {
			this.context = context;
		}

		public override void WriteFailure(string context, string testName) {
			WriteTestLabel(FailureColor, context, testName);
		}

		public override void WriteSuccess(IConeTest test) {
			WriteTestLabel(SuccessColor, test);
		}

		public override void WritePending(IConeTest test) {
			WriteTestLabel(PendingColor, test);
		}

		void WriteTestLabel(ConsoleColor color, IConeTest test) {
			WriteTestLabel(color, test.TestName.Context, test.TestName.Name);
		}

		void WriteTestLabel(ConsoleColor color, string contextName, string testName) {
			var parts = contextName.Split('.');
			var skip = 0;
			while(skip != context.Count && context[skip] == parts[skip])
				++skip;
			context.Clear();
			context.AddRange(parts);
			for(; skip != context.Count; ++skip)
				Write(InfoColor, "{0}{1}\n", new string(' ', skip << 1), context[skip]);
			Write(color, "{0}* {1}\n", new string(' ', skip << 1), testName);
		}
	}

	class TestNameConsoleLoggerWriter : ConsoleLoggerWriter
	{
		public override void WriteFailure(string context, string testName) {
			WriteTestName(FailureColor, context, testName);
		}

		public override void WriteSuccess(IConeTest test) {
			WriteTestName(SuccessColor, test);
		}

		public override void WritePending(IConeTest test) {
			WriteTestName(PendingColor, test);
		}

		void WriteTestName(ConsoleColor color, IConeTest test) {
			WriteTestName(color, test.TestName.Context, test.TestName.Name);
		}

		void WriteTestName(ConsoleColor color, string contextName, string testName) {
			Write(color, "{0}.{1}\n", contextName, testName.Replace("\n", "\\n").Replace("\r", ""));
		}
	}

    public class ConsoleLogger : ITestLogger
    {
        readonly IConeTest test;
		readonly ConsoleLoggerWriter writer;
		bool hasFailed;

        public ConsoleLogger(IConeTest test, ConsoleLoggerWriter writer) {
            this.test = test;
			this.writer = writer;
        }

        public void Failure(ConeTestFailure failure) {
			if(hasFailed)
				return;
			hasFailed = true;
			writer.WriteFailure(failure.Context, failure.TestName);
        }

        public void Success() {
			writer.WriteSuccess(test);
		}

        public void Pending(string reason) {
			writer.WritePending(test);
        }

        public void Skipped() { }

		public void EndTest() { }
    }
}
