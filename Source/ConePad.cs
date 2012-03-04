using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cone.Runners;

namespace Cone
{
    public static class ConePad
    {
        public static void RunTests() {
            Verify.GetPluginAssemblies = () => new[]{ typeof(Verify).Assembly };
            RunTests(new ConsoleLogger(), Assembly.GetCallingAssembly().GetTypes().Where(ConePadSuiteBuilder.SupportedType));
        }

        public static void RunTests(TextWriter output, IEnumerable<Assembly> assemblies) {
            Verify.GetPluginAssemblies = () => assemblies.Concat(new[]{ typeof(Verify).Assembly });
            RunTests(new ConsoleLogger(), assemblies.SelectMany(x => x.GetTypes()).Where(ConePadSuiteBuilder.SupportedType));
        }

        public static void RunTests(params Type[] suiteTypes) {
            RunTests(new ConsoleLogger(), suiteTypes);
        }

        public static void RunTests(IConeLogger log, IEnumerable<Type> suites) {
            log.Info("Running tests!\n----------------------------------\n");
            var runner = new SimpleConeRunner();
            runner.RunTests(log, suites);
        }
    }
}
