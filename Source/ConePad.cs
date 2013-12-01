using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Core;
using Cone.Runners;

namespace Cone
{
    public static class ConePad
    {
        class ConePadLogger : ISessionLogger, ISuiteLogger, ITestLogger
        {
            int failureCount;
            TextWriter Output { get { return Console.Out; } }

            public void BeginSession() { failureCount = 0; }

            public ISuiteLogger BeginSuite(IConeSuite suite) {
                return this;
            }

            public void EndSuite() { }

            public void EndSession() { }

            public ITestLogger BeginTest(IConeTest test) {
                return this;
            }

            public void WriteInfo(Action<TextWriter> output) {
                output(Output);
            }

            public void Failure(ConeTestFailure failure) {                
                Output.WriteLine(" {0}) {1}", ++failureCount, failure.Context);
                Output.WriteLine("\t\t{0}: {1}", failure.TestName, failure.Message);
            }

            public void Success() {
                Output.Write(".");
            }

            public void Pending(string reason) {
                Output.Write("?");
            }

            public void Skipped() { }

			public void EndTest() { }
        }

        public static void RunTests() {
            Check.GetPluginAssemblies = () => new[]{ typeof(Check).Assembly };
            RunTests(Assembly.GetCallingAssembly().GetTypes());
        }

        public static void RunTests(IEnumerable<Assembly> assemblies) {
            Check.GetPluginAssemblies = () => assemblies.Concat(new[]{ typeof(Check).Assembly });
            RunTests(assemblies.SelectMany(x => x.GetTypes()));
        }

        public static void RunTests(params Type[] suiteTypes) {
            RunTests(suiteTypes.AsEnumerable());
        }

        public static void RunTests(IEnumerable<Type> suites) {
			var log = new ConePadLogger();
            log.WriteInfo(writer => writer.WriteLine("Running tests!\n----------------------------------"));
            SimpleConeRunner.RunTests(log, suites);
        }
    }
}
