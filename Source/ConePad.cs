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
            TextWriter Output { get { return Console.Out; } }

            public void Info(string format, params object[] args) {
                Output.Write(format, args);
            }

            public void Failure(ConeTestFailure failure) {                
                Output.WriteLine(" {0}) {1}", failure.SequenceNumber, failure.Context);
                Output.WriteLine("\t\t{0}: {1}", failure.TestName, failure.Message);
            }

            public void Success(IConeTest test) {
                Info(".");
            }

            public void Pending(IConeTest test) {
                Info("?");
            }
        }

        public static void RunTests() {
            Verify.GetPluginAssemblies = () => new[]{ typeof(Verify).Assembly };
            RunTests(new ConePadLogger(), Assembly.GetCallingAssembly().GetTypes().Where(ConePadSuiteBuilder.SupportedType));
        }

        public static void RunTests(TextWriter output, IEnumerable<Assembly> assemblies) {
            Verify.GetPluginAssemblies = () => assemblies.Concat(new[]{ typeof(Verify).Assembly });
            RunTests(new ConePadLogger(), assemblies.SelectMany(x => x.GetTypes()).Where(ConePadSuiteBuilder.SupportedType));
        }

        public static void RunTests(params Type[] suiteTypes) {
            RunTests(new ConePadLogger(), suiteTypes);
        }

        public static void RunTests(IConeLogger log, IEnumerable<Type> suites) {
            log.Info("Running tests!\n----------------------------------\n");
            var runner = new SimpleConeRunner(log);
            runner.RunTests(suites);
        }
    }
}
