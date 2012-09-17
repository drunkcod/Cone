using System;
using Cone.Core;

namespace Cone.Runners
{
    public enum LoggerVerbosity {
        Default,
        TestName
    }

    public class ConsoleLogger : IConeLogger
    {
		string[] context = new string[0];

		public void Info(string format, params object[] args) {
            Console.Out.Write(format, args);
        }

        public LoggerVerbosity Verbosity;
		public ConsoleColor SuccessColor = ConsoleColor.Green;

		public void BeginSession() { }
		public void EndSession() { }

        public void Failure(ConeTestFailure failure) {
			switch(Verbosity) {
				case LoggerVerbosity.Default: Info("F"); break;
                case LoggerVerbosity.TestName: WriteTestName(failure.Context, failure.TestName, ConsoleColor.Red); break;
			}
        }

        public void Success(IConeTest test) {
            switch(Verbosity) {
                case LoggerVerbosity.Default: Info("."); break;
                case LoggerVerbosity.TestName: WriteTestName(test, SuccessColor); break;
            }
        }

        public void Pending(IConeTest test) {
			switch(Verbosity) {
				case LoggerVerbosity.Default: Info("?"); break;
                case LoggerVerbosity.TestName: WriteTestName(test, ConsoleColor.Yellow); break;
			}
        }

		void WriteTestName(IConeTest test, ConsoleColor color) {
			WriteTestName(test.Name.Context, test.Name.Name, color);
		}

		void WriteTestName(string contextName, string testName, ConsoleColor color) {
			var parts = contextName.Split('.');
			var skip = 0;
			while(skip != context.Length && context[skip] == parts[skip])
				++skip;
			context = parts;
			for(; skip != context.Length; ++skip)
				Info("{0}{1}\n", new string(' ', skip << 1), context[skip]);
			var tmp = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Info("{0}* {1}\n", new string(' ', skip << 1), testName);
			Console.ForegroundColor = tmp;
		}
    }
}
