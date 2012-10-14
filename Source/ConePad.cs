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
        class ConePadLogger : IConeLogger
        {
            int failureCount;
            TextWriter Output { get { return Console.Out; } }

            public void BeginSession() { failureCount = 0; }
			public void EndSession() { }

            public void WriteInfo(Action<TextWriter> output) {
                output(Output);
            }

            public void Failure(ConeTestFailure failure) {                
                Output.WriteLine(" {0}) {1}", ++failureCount, failure.Context);
                Output.WriteLine("\t\t{0}: {1}", failure.TestName, failure.Message);
            }

            public void Success(IConeTest test) {
                Output.Write(".");
            }

            public void Pending(IConeTest test) {
                Output.Write("?");
            }

            public void Skipped(IConeTest test) { }
        }

		static ConePadSuiteBuilder SuiteBuilder = new ConePadSuiteBuilder();

        public static void RunTests() {
            Verify.GetPluginAssemblies = () => new[]{ typeof(Verify).Assembly };
            RunTests(new ConePadLogger(), Assembly.GetCallingAssembly().GetTypes());
        }

        public static void RunTests(TextWriter output, IEnumerable<Assembly> assemblies) {
            Verify.GetPluginAssemblies = () => assemblies.Concat(new[]{ typeof(Verify).Assembly });
            RunTests(new ConePadLogger(), assemblies.SelectMany(x => x.GetTypes()));
        }

        public static void RunTests(params Type[] suiteTypes) {
            RunTests(new ConePadLogger(), suiteTypes);
        }

        public static void RunTests(IConeLogger log, IEnumerable<Type> suites) {
            log.WriteInfo(writer => writer.WriteLine("Running tests!\n----------------------------------"));
        	new SimpleConeRunner().RunTests(new TestSession(log), suites);
        }
    }
}
