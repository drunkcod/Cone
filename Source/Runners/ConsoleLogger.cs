using System;
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

    public class ConsoleSessionLogger : ISessionLogger
    {
        public void WriteInfo(Action<TextWriter> output) {
            output(Console.Out);
        }

        public ConsoleLoggerSettings Settings = new ConsoleLoggerSettings();

        public void BeginSession() { }

        public IConeLogger BeginTest(IConeTest test) {
            return new ConsoleLogger(test) {
                Settings = Settings
            };
        }

        public void EndSession() { }
    }

    public class ConsoleLogger : IConeLogger
    {
        readonly IConeTest test;

        public ConsoleLogger(IConeTest test) {
            this.test = test;
        }

        public ConsoleLoggerSettings Settings;

        LoggerVerbosity Verbosity { get { return Settings.Verbosity; } }
        ConsoleColor SuccessColor { get { return Settings.SuccessColor; } }
        string[] Context {
            get { return Settings.Context; }
            set { Settings.Context = value; }
        }

        public void Failure(ConeTestFailure failure) {
			switch(Verbosity) {
				case LoggerVerbosity.Default: Write("F"); break;
				case LoggerVerbosity.TestNames: WriteTestName(failure.Context, failure.TestName, ConsoleColor.Red); break;
                case LoggerVerbosity.Labels: WriteTestLabel(failure.Context, failure.TestName, ConsoleColor.Red); break;
			}
        }

        public void Success() {
            switch(Verbosity) {
                case LoggerVerbosity.Default: Write("."); break;
                case LoggerVerbosity.TestNames: WriteTestName(test, SuccessColor); break;
                case LoggerVerbosity.Labels: WriteTestLabel(test, SuccessColor); break;
            }
        }

        public void Pending() {
			switch(Verbosity) {
				case LoggerVerbosity.Default: Write("?"); break;
                case LoggerVerbosity.TestNames: WriteTestName(test, ConsoleColor.Yellow); break;
                case LoggerVerbosity.Labels: WriteTestLabel(test, ConsoleColor.Yellow); break;
			}
        }

        public void Skipped() { }

		void WriteTestName(IConeTest test, ConsoleColor color) {
			WriteTestName(test.Name.Context, test.Name.Name, color);
		}

		void WriteTestName(string contextName, string testName, ConsoleColor color) {
			var tmp = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Write("{0}.{1}\n", contextName, testName.Replace("\n", "\\n").Replace("\r", ""));
			Console.ForegroundColor = tmp;
		}

		void WriteTestLabel(IConeTest test, ConsoleColor color) {
			WriteTestLabel(test.Name.Context, test.Name.Name, color);
		}

		void WriteTestLabel(string contextName, string testName, ConsoleColor color) {
			var parts = contextName.Split('.');
			var skip = 0;
			while(skip != Context.Length && Context[skip] == parts[skip])
				++skip;
			Context = parts;
			for(; skip != Context.Length; ++skip)
				Write("{0}{1}\n", new string(' ', skip << 1), Context[skip]);
			var tmp = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Write("{0}* {1}\n", new string(' ', skip << 1), testName);
			Console.ForegroundColor = tmp;
		}

		void Write(string format, params object[] args) {
			Console.Out.Write(format, args);
		}
    }
}
