using System;

namespace Cone.Runners
{
    public class ConsoleLogger : IConeLogger
    {
        public void Info(string format, object[] args) {
            Console.Out.Write(format, args);
        }

        public void Failure(ConeTestFailure failure) {
            Console.Out.WriteLine("{0}({1}) - {2}", failure.File, failure.Line, failure.Context);
            Console.Out.WriteLine("{0}: {1}", failure.TestName, failure.Message);
        }
    }
}
