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

    public class ConsoleSessionLogger : ISessionLogger, ISuiteLogger
    {
        public void WriteInfo(Action<TextWriter> output) {
            output(Console.Out);
        }

        public ConsoleLoggerSettings Settings = new ConsoleLoggerSettings();

        public void BeginSession() { }

        public ISuiteLogger BeginSuite(IConeSuite suite) {
            return new ConsoleSessionLogger {
                Settings = new ConsoleLoggerSettings {
                    Verbosity = Settings.Verbosity,
                    SuccessColor = Settings.SuccessColor,
                }
            };
        }

        public void EndSuite() { }

        public ITestLogger BeginTest(IConeTest test) {
            return new ConsoleLogger(test) {
                Settings = Settings,
            };
        }

        public void EndSession() { }
    }

    public class ConsoleLogger : ITestLogger
    {
        readonly IConeTest test;
		bool hasFailed;

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
			if(hasFailed)
				return;
			hasFailed = true;
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

        public void Pending(string reason) {
			switch(Verbosity) {
				case LoggerVerbosity.Default: Write("?"); break;
                case LoggerVerbosity.TestNames: WriteTestName(test, ConsoleColor.Yellow); break;
                case LoggerVerbosity.Labels: WriteTestLabel(test, ConsoleColor.Yellow); break;
			}
        }

        public void Skipped() { }

		public void EndTest() { }

		void WriteTestName(IConeTest test, ConsoleColor color) {
			WriteTestName(test.TestName.Context, test.TestName.Name, color);
		}

		void WriteTestName(string contextName, string testName, ConsoleColor color) {
			Write(color, "{0}.{1}\n", contextName, testName.Replace("\n", "\\n").Replace("\r", ""));
		}

		void WriteTestLabel(IConeTest test, ConsoleColor color) {
			WriteTestLabel(test.TestName.Context, test.TestName.Name, color);
		}

		void WriteTestLabel(string contextName, string testName, ConsoleColor color) {
			var parts = contextName.Split('.');
			var skip = 0;
			while(skip != Context.Length && Context[skip] == parts[skip])
				++skip;
			Context = parts;
			for(; skip != Context.Length; ++skip)
				Write("{0}{1}\n", new string(' ', skip << 1), Context[skip]);
			Write(color, "{0}* {1}\n", new string(' ', skip << 1), testName);
		}

        void Write(ConsoleColor color, string format, params object[] args) {
            lock (Console.Out) {
                var tmp = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.Out.Write(format, args);
                Console.ForegroundColor = tmp;
            }
        }
        
        void Write(string format, params object[] args) {
			Console.Out.Write(format, args);
		}
    }
}
