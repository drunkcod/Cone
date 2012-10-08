using System;
using Cone.Core;

namespace Cone.Runners
{
    public enum LoggerVerbosity {
        Default,
		TestNames,
        Labels
    }

    public class ConsoleLogger : IConeLogger
    {
		string[] context = new string[0];

		public void Info(string format, params object[] args) {
            Console.Out.WriteLine(format, args);
        }

        public LoggerVerbosity Verbosity;
		public ConsoleColor SuccessColor = ConsoleColor.Green;

		public void BeginSession() { }
		public void EndSession() { }

        public void Failure(ConeTestFailure failure) {
			switch(Verbosity) {
				case LoggerVerbosity.Default: Write("F"); break;
				case LoggerVerbosity.TestNames: WriteTestName(failure.Context, failure.TestName, ConsoleColor.Red); break;
                case LoggerVerbosity.Labels: WriteTestLabel(failure.Context, failure.TestName, ConsoleColor.Red); break;
			}
        }

        public void Success(IConeTest test) {
            switch(Verbosity) {
                case LoggerVerbosity.Default: Write("."); break;
                case LoggerVerbosity.TestNames: WriteTestName(test, SuccessColor); break;
                case LoggerVerbosity.Labels: WriteTestLabel(test, SuccessColor); break;
            }
        }

        public void Pending(IConeTest test) {
			switch(Verbosity) {
				case LoggerVerbosity.Default: Write("?"); break;
                case LoggerVerbosity.TestNames: WriteTestName(test, ConsoleColor.Yellow); break;
                case LoggerVerbosity.Labels: WriteTestLabel(test, ConsoleColor.Yellow); break;
			}
        }

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
			while(skip != context.Length && context[skip] == parts[skip])
				++skip;
			context = parts;
			for(; skip != context.Length; ++skip)
				Write("{0}{1}\n", new string(' ', skip << 1), context[skip]);
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
