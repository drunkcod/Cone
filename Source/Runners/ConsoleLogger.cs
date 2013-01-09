using System;
using System.Diagnostics;
using System.IO;
using Cone.Core;

namespace Cone.Runners
{
    public enum LoggerVerbosity {
        Default,
		TestNames,
        Labels
    }

    public class ConsoleLoggerSettings
    {
        internal string[] Context = new string[0];
        public LoggerVerbosity Verbosity;
        public ConsoleColor SuccessColor = ConsoleColor.Green;
    }

    public class ConsoleSessionLogger : ISessionLogger, ISuiteLogger
    {
		readonly ConsoleLoggerSettings settings;
		readonly ConsoleLoggerWriter writer;

		public ConsoleSessionLogger(ConsoleLoggerSettings settings) {
			this.settings = settings;
			switch(settings.Verbosity) {
				case LoggerVerbosity.Default: writer = new ConsoleLoggerWriter(); break;
				case LoggerVerbosity.Labels: writer = new LabledConsoleLoggerWriter(); break;
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
            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Out.Write(format, args);
            Console.ForegroundColor = tmp;
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
		string[] context = new string[0];

		public override void WriteFailure(string context, string testName) {
			WriteTestLabel(FailureColor, context, testName);
		}

		public override void WriteSuccess(IConeTest test) {
			WriteTestLabel(SuccessColor, test);
		}

		public override void WritePending(IConeTest test) {
			WriteTestLabel(PendingColor, test);
		}

		void WriteTestLabel(ConsoleColor color, IConeTest test)
		{
			WriteTestLabel(color, test.TestName.Context, test.TestName.Name);
		}

		void WriteTestLabel(ConsoleColor color, string contextName, string testName)
		{
			var parts = contextName.Split('.');
			var skip = 0;
			while(skip != context.Length && context[skip] == parts[skip])
				++skip;
			context = parts;
			for(; skip != context.Length; ++skip)
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

		void WriteTestName(ConsoleColor color, IConeTest test)
		{
			WriteTestName(color, test.TestName.Context, test.TestName.Name);
		}

		void WriteTestName(ConsoleColor color, string contextName, string testName)
		{
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
