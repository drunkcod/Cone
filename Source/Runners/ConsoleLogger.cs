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
        public void Info(string format, params object[] args) {
            Console.Out.Write(format, args);
        }

        public LoggerVerbosity Verbosity;

        public void Failure(ConeTestFailure failure) {
            Console.Out.WriteLine("{0}. {1}({2}) - {3}", failure.SequenceNumber, failure.File, failure.Line, failure.Context);
            Console.Out.WriteLine("{0}: {1}", failure.TestName, failure.Message);
        }

		string[] context = new string[0];

        public void Success(IConeTest test) {
            switch(Verbosity) {
                case LoggerVerbosity.Default: Info("."); break;
                case LoggerVerbosity.TestName: 
					var parts = test.Name.Context.Split('.');
					var skip = 0;
					while(skip != context.Length && context[skip] == parts[skip])
						++skip;
					context = parts;
					for(; skip != context.Length; ++skip)
						Info("{0}{1}\n", new string(' ', skip << 1), context[skip]);
					Info("{0}{1}\n", new string(' ', skip << 1), test.Name.Name); break;
            }
        }

        public void Pending(IConeTest test) {
            Info("?");
        }
    }
}
