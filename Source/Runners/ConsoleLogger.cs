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

        public void Success(IConeTest test) {
            switch(Verbosity) {
                case LoggerVerbosity.Default: Info("."); break;
                case LoggerVerbosity.TestName: Info("{0}\n", test.Name.FullName); break;
            }
        }

        public void Pending(IConeTest test) {
            Info("?");
        }
    }
}
